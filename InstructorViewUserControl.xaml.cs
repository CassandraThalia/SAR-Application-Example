//This file contains the code behind the Spreadsheet Table User Control. This is the user control which displays the data which has been uploaded from the spreadsheet.
//It is launched from the Spreadsheet Upload Selector Dialog in the Content Dialogs folder.

using SAR.Models;
using SAR.ViewModels;
using SAR.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SAR.ContentDialogs;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using Windows.UI.Popups;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SAR
{
    public sealed partial class SpreadsheetTableUserControl : UserControl
    {
        public static ListView SpreadsheetRows;

        private StackPanel SideBar = new StackPanel();

        //Constructor contains one parameter, Highest Grade, which is passed from the Spreadsheet Upload Selector Dialog. The user has the option to change this number in a text box.
        public SpreadsheetTableUserControl(string highestGrade)
        {
            this.InitializeComponent();

            try
            {
                MainPage.SpreadsheetInProgress = true;
                SpreadsheetRows = SpreadsheetTable;
                FinalGrade.Text = "Final Grade / " + highestGrade;
                SetButton();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error initializing spreadsheet table info: " + e);
            }

            try
            {
                //Build legend programatically and add it to the Main Page Side Bar
                // *** Note about background colors: there is no reason for the first two to be gradients, but SolidColorBrush was causing weird hover behavior.
                // *** Changing them to LinearGradientBrush fixed the problem, for some reason.

                StackPanel redPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["str-red"] };
                redPanel.Padding = new Thickness(5);
                redPanel.Margin = new Thickness(10, 20, 10, 10);
                redPanel.Width = 220;
                redPanel.CornerRadius = new CornerRadius(3);
                TextBlock redTextBlock = new TextBlock { Text = "Students' current final grade is less than 60% of the highest possible grade" };
                redTextBlock.TextWrapping = TextWrapping.Wrap;
                redTextBlock.TextAlignment = TextAlignment.Center;
                redTextBlock.FontSize = 12;
                redTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["black"];
                redPanel.Children.Add(redTextBlock);
                SideBar.Children.Add(redPanel);

                StackPanel yellowPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"] };
                yellowPanel.Padding = new Thickness(5);
                yellowPanel.Margin = new Thickness(10, 0, 10, 10);
                yellowPanel.Width = 220;
                yellowPanel.CornerRadius = new CornerRadius(3);
                TextBlock yellowTextBlock = new TextBlock { Text = "Student did not submit the selected assessment (if applicable)" };
                yellowTextBlock.TextWrapping = TextWrapping.Wrap;
                yellowTextBlock.TextAlignment = TextAlignment.Center;
                yellowTextBlock.FontSize = 12;
                yellowPanel.Children.Add(yellowTextBlock);
                SideBar.Children.Add(yellowPanel);

                StackPanel stripedPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["striped"] };
                stripedPanel.Padding = new Thickness(5);
                stripedPanel.Margin = new Thickness(10, 0, 10, 10);
                stripedPanel.Width = 220;
                stripedPanel.CornerRadius = new CornerRadius(3);
                TextBlock stirpedTextBlock = new TextBlock { Text = "Both warnings are applicable" };
                stirpedTextBlock.TextWrapping = TextWrapping.Wrap;
                stirpedTextBlock.TextAlignment = TextAlignment.Center;
                stirpedTextBlock.FontSize = 12;
                stripedPanel.Children.Add(stirpedTextBlock);
                SideBar.Children.Add(stripedPanel);

                //Add Save Button to Main Page Side Bar
                Button saveButton = new Button { Content = "Save Changes" };
                saveButton.Padding = new Thickness(15);
                saveButton.Margin = new Thickness(0, 30, 0, 0);
                saveButton.Background = (SolidColorBrush)Application.Current.Resources["green"];
                saveButton.Foreground = (SolidColorBrush)Application.Current.Resources["white"];
                saveButton.FontSize = 18;
                saveButton.Click += SaveButton_Click;
                SideBar.Children.Add(saveButton);

                //Add Exit Button to Main Page Side Bar
                Button exitButton = new Button { Content = "Exit Without Saving" };
                exitButton.Padding = new Thickness(10);
                exitButton.Margin = new Thickness(0, 20, 0, 0);
                exitButton.Background = (SolidColorBrush)Application.Current.Resources["gray"];
                exitButton.FontSize = 14;
                exitButton.Click += ExitButton_Click;
                SideBar.Children.Add(exitButton);

                MainPage.SideBar.Children.Add(SideBar);

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error adding legend and save button to side bar: " + e);
            }
        }


        private void SetButton()
        {
            foreach (CSVParsedObject obj in SpreadsheetTableViewModel.CSVData)
            {
                obj.Student.ButtonBG = (SolidColorBrush)Application.Current.Resources["NSCCblue"];
                obj.Student.ButtonFG = (SolidColorBrush)Application.Current.Resources["white"];
                obj.Student.ButtonMsg = "Leave Note";
            }
        }

        //This function sets the background color of the listview row by looping throug the rows. 
        //It is based on logic found in the SpreadsheetTableViewModel CheckStudentStatus function, which sets the StudentInDanger and AssessmentNotSubmitted bools.
        //It also uses the NoteAdded bool to change the button color & text once a note has been added (meaning that the flagged student has been addressed) and the table is refreshed.
        private void SpreadsheetTable_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            try
            {
                ObservableCollection<CSVParsedObject> CSVData = SpreadsheetTableViewModel.CSVData;
                string noteAdded = "Note Saved!";

                if (CSVData[args.ItemIndex].StudentInDanger && CSVData[args.ItemIndex].AssessmentNotSubmitted && !CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["striped"];
                }
                else if (CSVData[args.ItemIndex].StudentInDanger && CSVData[args.ItemIndex].AssessmentNotSubmitted && CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["striped"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonMsg = noteAdded;
                }
                else if (CSVData[args.ItemIndex].StudentInDanger && !CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-red"];
                }
                else if (CSVData[args.ItemIndex].StudentInDanger && CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-red"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonMsg = noteAdded;

                }
                else if (CSVData[args.ItemIndex].AssessmentNotSubmitted && !CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"];
                }
                else if (CSVData[args.ItemIndex].AssessmentNotSubmitted && CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonMsg = noteAdded;
                }
                else if (!CSVData[args.ItemIndex].StudentInDanger && !CSVData[args.ItemIndex].AssessmentNotSubmitted && CSVData[args.ItemIndex].Student.NoteAdded)
                {
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    SpreadsheetTableViewModel.CSVData[args.ItemIndex].Student.ButtonMsg = noteAdded;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error changing row background colors: " + e);
            }

        }

        //Launch the StudentDetailsDialog, found in the ContentDialogs folder, when the Leave Note button is clicked
        private async void StudentNotesButton_Click(object sender, RoutedEventArgs e)
        {
            //JSONObject data = (JSONObject)(sender as FrameworkElement).DataContext;
            CSVParsedObject data = (CSVParsedObject)(sender as FrameworkElement).DataContext;
            Student studentOfNote = data.Student;
            //SpreadsheetTableViewModel.CompareStudentEnrollmentStatus();

            // Determine if student is in danger
            bool isFailing = data.StudentInDanger;
            bool notSubmitted = data.AssessmentNotSubmitted;

            int warningLvl = 0;
            try
            {
                if (isFailing && notSubmitted)
                {
                    warningLvl = 3;
                }
                else if (notSubmitted)
                {
                    warningLvl = 1;
                }
                else if (isFailing)
                {
                    warningLvl = 2;
                }
            }
            catch (Exception e2)
            {
                MessageDialog errorMsg = new MessageDialog("Error getting student warning level: " + e2);
                await errorMsg.ShowAsync();
            }

            try
            {
                StudentDetailsDialog stdntDialog = new StudentDetailsDialog(studentOfNote, warningLvl);
                await stdntDialog.ShowAsync();
            }
            catch (Exception e3)
            {
                MessageDialog errorMsg = new MessageDialog("Error launching student detail dialog: " + e3);
                await errorMsg.ShowAsync();
            }
        }

        //When the save button is clicked, we first check if there are any flagged students that have not been addressed.
        //After this is handled (if necessary), we call the ConfirmSave function, which sends the final grade info to the Database.
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //Loop through current collection of JSON objects
            //If student has been flagged as in danger or as having an assessment not submitted, show confirmation dialog before returning to upload screen
            ObservableCollection<CSVParsedObject> CSVData = SpreadsheetTableViewModel.CSVData;
            bool flaggedStudentsWithoutNotes = false;
            try
            {
                foreach (CSVParsedObject row in CSVData)
                {
                    if ((row.StudentInDanger == true && row.Student.NoteAdded == false) || (row.AssessmentNotSubmitted == true && row.Student.NoteAdded == false))
                    {
                        flaggedStudentsWithoutNotes = true;
                    }
                }
            }
            catch (Exception e2)
            {
                MessageDialog errorMsg = new MessageDialog("Error checking for flagged students: " + e2);
                await errorMsg.ShowAsync();
            }

            //If there are flagged students without notes, check before saving
            try
            {
                if (flaggedStudentsWithoutNotes == true)
                {
                    ContentDialog notesNotAddedDialog = new ContentDialog()
                    {
                        Title = "Notes Not Added",
                        Content = "You have not added notes to every flagged student",
                        PrimaryButtonText = "Return",
                        SecondaryButtonText = "Continue",
                        PrimaryButtonStyle = (Style)Application.Current.Resources["secondaryButtonStyle"],
                        SecondaryButtonStyle = (Style)Application.Current.Resources["primaryButtonStyle"]
                    };

                    //If they choose to continue anyway, proceed with database insert & return to upload screen
                    ContentDialogResult result1 = await notesNotAddedDialog.ShowAsync();
                    if (result1 == ContentDialogResult.Secondary)
                    {
                        ConfirmSave(CSVData);
                    }
                }
                //If all flagged students have notes, proceed with database insert & return to upload screen
                else
                {
                    ConfirmSave(CSVData);
                }
            }
            catch (Exception e3)
            {
                MessageDialog errorMsg = new MessageDialog("Error checking flags & confirming save: " + e3);
                await errorMsg.ShowAsync();
            }
        }

        //This button allows the user to exit the Spreadsheet Table User Control without first sending the students' final grades to the Database
        private async void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog notesNotAddedDialog = new ContentDialog()
                {
                    Title = "Confirm Exit",
                    Content = "Are you sure you want to exit without saving your data?",
                    PrimaryButtonText = "No",
                    SecondaryButtonText = "Yes",
                    PrimaryButtonStyle = (Style)Application.Current.Resources["secondaryButtonStyle"],
                    SecondaryButtonStyle = (Style)Application.Current.Resources["primaryButtonStyle"]
                };
                ContentDialogResult result = await notesNotAddedDialog.ShowAsync();
                //If yes, close the Spreadsheet Table User Control and return to the Upload File User Control
                if (result == ContentDialogResult.Secondary)
                {
                    ReturnToUpload();
                }
            }
            //If no, close this dialog and return to Spreadsheet Table User Control
            catch (Exception e2)
            {
                MessageDialog errorMsg = new MessageDialog("Error exiting spreadsheet without saving: " + e2);
                await errorMsg.ShowAsync();
            }
        }

        //This function closes the current Spreadsheet Table User Control and relaunches to the Upload File User Control, so the user may begin a new upload
        private async void ReturnToUpload()
        {
            try
            {
                MainPage.SpreadsheetInProgress = false;
                MainPage.SideBar.Children.Remove(SideBar);
                MainPage.mainCC.Content = new UploadFileUserControl();
                MainPage.mainCC.VerticalAlignment = VerticalAlignment.Center;
                MainPage.mainCC.HorizontalAlignment = HorizontalAlignment.Center;
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error returning to upload screen: " + e);
                await errorMsg.ShowAsync();
            }
        }

        //This function uses two functions from the SpreadsheetTableViewModel to send updated student grades to the Database.
        //Before sending the grades, it first makes sure that there is an entry in the course_student pivot table for each student with this particular course
        //(essentially, we first make sure that the student is "enrolled" in the course)
        private async void ConfirmSave(ObservableCollection<CSVParsedObject> CSVData)
        {
            try
            {
                SpreadsheetTableViewModel.CreateEnrollmentIfNecessary(CSVData);
                SpreadsheetTableViewModel.UpdateStudentFinalGradesWarningLvl(CSVData);
                ContentDialog saveDialog = new ContentDialog()
                {
                    Title = "Data Saved",
                    Content = "You saved your data!",
                    PrimaryButtonText = "OK",
                    PrimaryButtonStyle = (Style)Application.Current.Resources["primaryButtonStyle"]
                };
                await saveDialog.ShowAsync();
                ReturnToUpload();
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error confirming save: " + e);
                await errorMsg.ShowAsync();
            }
        }


        //This function refreshes the table so that we can remove the background colors from flagged students who have been attended to
        //It is called from within the Student Details Dialog, so that the table refreshes at the same time as the dialog closes.
        public static async void RefreshTable()
        {
            try
            {
                // Inspiration for this is from here: https://stackoverflow.com/questions/49370365/how-to-refresh-a-user-control-every-time-a-button-is-clicked
                SpreadsheetRows.ItemsSource = null;
                SpreadsheetRows.ItemsSource = SpreadsheetTableViewModel.CSVData;
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error refreshing table: " + e);
                await errorMsg.ShowAsync();
            }
        }


    }
}

//Resource for ContainerContentChangingEvent
//https://stackoverflow.com/questions/35346001/uwp-how-to-change-background-color-of-listview-item-based-on-its-value
//Resource for stripes
//https://stackoverflow.com/questions/28986368/add-striped-background-in-windows-store-app