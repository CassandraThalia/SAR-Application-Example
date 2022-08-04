using SAR.ContentDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SAR.ViewModels;
using SAR.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using SAR.Repository;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SAR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static User User { get; set; }
        public static ContentControl mainCC { get; set; }
        public static List<Student> UsersStudents { get; set; }        

        public static bool IsCurrentlyInstructor { get; set; }

        public static bool SpreadsheetInProgress { get; set; }

        public static StackPanel SideBar { get; set; }

        public static string HashedPass { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            mainCC = mainDisplayFrame;
            SideBar = sideBar;
            IsCurrentlyInstructor = true;

            Logo.Source = new BitmapImage(new Uri("ms-appx:///Assets/SAR_Logo_Blue.png"));

            // call an async function to show login page and get User's info
            Task task = ShowLogin();

            UploadFileUserControl ufuc = new UploadFileUserControl();
            mainCC.HorizontalAlignment = HorizontalAlignment.Center;
            mainCC.VerticalAlignment = VerticalAlignment.Center;
            mainCC.Content = ufuc;
        }

        public async Task ShowLogin()
        {
            Login login = new Login();
            await login.ShowAsync();

            if (login.Result == SignInResult.SignInOK)
            {
                User = login.User;
                // gets user's info to update User object
                Repository.UserRepository.GetUserInfo(User);

                // prepares a list of students related to the User for searching
                UsersStudents = Repository.StudentRepository.GetStudentsByUser(User);

                AddInitUploadButton(User);

                UserName.Text = User.FullName;

                //TEMPORARY PASSOWORD CHECK
                //This checks for the user's last name + 123
                string tempPass = User.FullName.Split(' ').Skip(1).FirstOrDefault() + "123";
                //Hash the user's temp password
                tempPass = UserRepository.GetHashedStr(tempPass);
                //If the hashed password they entered matches thier hashed temp password, prompt password change
                if (HashedPass == tempPass)
                {
                    Task task2 = ChangeTempPass();
                }
            }
        }

        public async Task ChangeTempPass()
        {
            ChangePasswordDialog cpd = new ChangePasswordDialog(User, HashedPass);
            await cpd.ShowAsync();
        }

        private void ChangeToAdvisorView()
        {
            //Change toggle buttons & home button background color
            teacherButton.Background = (SolidColorBrush)Application.Current.Resources["transparent"];
            teacherButton.Foreground = (SolidColorBrush)Application.Current.Resources["black"];

            advisorButton.Background = (SolidColorBrush)Application.Current.Resources["orange"];
            advisorButton.Foreground = (SolidColorBrush)Application.Current.Resources["white"];

            Logo.Source = new BitmapImage(new Uri("ms-appx:///Assets/SAR_Logo_Orange.png"));

            //Remove anything from Side Bar
            SideBar.Children.Clear();

            // load the advisor view user control
            mainCC.Content = new AdvisorViewUserControl();
            SpreadsheetInProgress = false;
        }

        private async void advisorButton_Click(object sender, RoutedEventArgs e)
        {
            IsCurrentlyInstructor = false;

            if (SpreadsheetInProgress)
            {
                ContentDialog warning = new ContentDialog()
                {
                    Title = "Spreadsheet Info Not Saved",
                    Content = "You are navigating away from your spreadsheet upload without saving your data, continue?",
                    PrimaryButtonText = "No",
                    SecondaryButtonText = "Yes"
                };
                ContentDialogResult result = await warning.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    ChangeToAdvisorView();
                }
            }
            else
            {
                ChangeToAdvisorView();
            }
        }

        private void ChangeToInstructorView()
        {
            //Change toggle buttons & home button background color
            advisorButton.Background = (SolidColorBrush)Application.Current.Resources["transparent"];
            advisorButton.Foreground = (SolidColorBrush)Application.Current.Resources["black"];

            teacherButton.Background = (SolidColorBrush)Application.Current.Resources["NSCCblue"];
            teacherButton.Foreground = (SolidColorBrush)Application.Current.Resources["white"];

            Logo.Source = new BitmapImage(new Uri("ms-appx:///Assets/SAR_Logo_Blue.png"));

            //Remove anything from Side Bar
            SideBar.Children.Clear();

            // load the faculty upload report view
            mainCC.Content = new UploadFileUserControl();
            mainCC.HorizontalAlignment = HorizontalAlignment.Center;
            mainCC.VerticalAlignment = VerticalAlignment.Center;

            SpreadsheetInProgress = false;
        }
        private async void teacherButton_Click(object sender, RoutedEventArgs e)
        {
            IsCurrentlyInstructor = true;

            if (SpreadsheetInProgress)
            {
                ContentDialog warning = new ContentDialog()
                {
                    Title = "Spreadsheet Info Not Saved",
                    Content = "You are navigating away from your spreadsheet upload without saving your data, continue?",
                    PrimaryButtonText = "No",
                    SecondaryButtonText = "Yes"
                };
                ContentDialogResult result = await warning.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    ChangeToInstructorView();
                }
            }
            else
            {
                ChangeToInstructorView();
            }
        }

        // a list of strings to store User's students info in "wNum - fullname" format for searching purpose
        public List<string> UsersStudentsStr 
        { 
            get 
            {
                List<string> datasetToSearch = new List<string>();

                foreach (var stu in UsersStudents)
                {
                    string numAndName = stu.StudentID + " - " + stu.StudentFName + " " + stu.StudentLName;
                    datasetToSearch.Add(numAndName);
                }

                return datasetToSearch;
            }
        }
        private void searchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {                
                string inputProcessed;
                List<string> suggestion;

                // lower-case and trim string
                inputProcessed = sender.Text.ToLowerInvariant().Trim();

                // Use LINQ query to get all student that match filter text, as a list
                if (inputProcessed.Length > 0 && Char.IsLetter(inputProcessed[0]))
                {
                    // search by name
                    suggestion = UsersStudentsStr.Where(d => d.ToLowerInvariant().Contains(inputProcessed)).ToList();
                }
                else
                {                    
                    // search by wNum
                    suggestion = UsersStudentsStr.Where(d => d.ToLowerInvariant().StartsWith(inputProcessed)).ToList();
                }                

                //Set the ItemsSource to be your filtered dataset
                sender.ItemsSource = suggestion;
            }
        }
        private void searchTextBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
        }

        private async void searchTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string stuIdSelected;

            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                stuIdSelected = args.ChosenSuggestion.ToString().Substring(0, 7);
            }
            else
            {
                // Use args.QueryText to determine what to do.
                stuIdSelected = args.QueryText;
            }

            // to get the Student object
            Student stuToView = null;

            foreach (var stu in UsersStudents)
            {
                if (stu.StudentID.Equals(stuIdSelected)) { stuToView = stu; break; }
            }

            if (stuToView != null)
            {
                // to get the courses stats
                stuToView.CoursesGrades = Repository.StudentRepository.GetCoursesStats(stuToView);

                StudentDetailsDialog stdntDialog = new StudentDetailsDialog(stuToView, 0);
                await stdntDialog.ShowAsync();
            } 
            else
            {
                MessageDialog messageDialog = new MessageDialog("W number not found.", "Oops!");
                await messageDialog.ShowAsync();
            }

            // empty the search box
            sender.Text = string.Empty;
        }
        private async void InitUploadButton_Click(object sender, RoutedEventArgs e)
        {
            InitialUploadFileSelector initUploadBox = new InitialUploadFileSelector();
            await initUploadBox.ShowAsync();
        }

        private void AddInitUploadButton(User user)
        {
            // Add initial upload button if user is Michael Crocker (De facto System Admin)
            if (User != null)
            {
                if (User.WNumber == "1471139")
                {
                    InitUploadButton.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
