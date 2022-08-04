using SAR.Models;
using SAR.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SAR.ContentDialogs
{
    public sealed partial class StudentDetailsDialog : ContentDialog
    {

        public Student student { get; set; }
        public string studentFullName { get; set; }
        public User user { get; set; }

        // Codes current are: 0 - All fine; 1 - Assignment Not Submitted; 2 - Failing; 3 - Failing & Assignment Not Submitted
        private int StudentStatus = 0;
        public List<Course> CoursesList { get; set; }
        public StudentDetailsDialog(Student stdnt, int studentStatus)
        {
            this.InitializeComponent();

            try
            {
                student = stdnt;
                StudentStatus = studentStatus;
                studentFullName = student.StudentFName + " " + student.StudentLName;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error initializing student params: " + e);
            }

            try
            {
                // This switch is responsible for automatically adding values to textbox relating to the student's academic status
                switch (studentStatus)
                {
                    case 0:
                        break;
                    case 1:
                        NoteContentBox.Text = studentFullName + " has not submitted most recent assignment. ";
                        break;
                    case 2:
                        NoteContentBox.Text = studentFullName + " may be at risk of failing course. ";
                        break;
                    case 3:
                        NoteContentBox.Text = studentFullName + " may be at risk of failing course and has not submitted most recent assignment. ";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting student status: " + e);
            }

            try
            {
                //If not already done, load Student notes list with notes from Database
                if (student.NotesLoaded == false)
                {
                    AddNotesToStudent();
                    student.NotesLoaded = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting student notes: " + e);
            }

            try
            {
                student.UnreadNotes = "";
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error resetting student note message: " + e);
            }

            try
            {
                //Change look and content of dialog based on whether the user is currenly in faculty or advisor view
                if (MainPage.IsCurrentlyInstructor == true)
                {
                    TB1.Text = "Advisor Name";
                    StudentRepository.GetStudentAdvisor(student);

                    // Check if student has an Advisor
                    if (student.Advisor != null)
                    {
                        TB1Content.Text = student.Advisor.FullName;
                    }
                    else
                    {
                        TB1Content.Text = "Advisor Unknown";
                    }


                    TB2.Text = "Instructor Name";
                    TB2Content.Text = MainPage.User.FullName;

                    TB3.Text = "Student Info";
                    string yearOfStudy = StudentRepository.GetStudentYear(student);
                    TB3Content.Text = "Year of Study: " + yearOfStudy;

                    NewNoteGrid.Background = (SolidColorBrush)Application.Current.Resources["light-blue"];

                }
                else if (MainPage.IsCurrentlyInstructor == false)
                {
                    TB1.Text = "Student Info";
                    string yearOfStudy = StudentRepository.GetStudentYear(student);
                    TB1Content.Text = "Year of Study: " + yearOfStudy;

                    // show stats of courses instead
                    TB2.Text = "Recent Evaluations";
                    CoursesList = StudentRepository.GetCoursesStats(student);
                    // reorder the list with the problematic ones on top. ref: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=net-6.0
                    CoursesList.Sort(delegate (Course x, Course y)
                    {
                        if (x.HasWithdrawn && !y.HasWithdrawn) return -1;
                        else if (y.HasWithdrawn && !x.HasWithdrawn) return 1;
                        else return y.WarningLvl.CompareTo(x.WarningLvl);
                    });

                    // Truncate the decimal places within CourseList
                    // This trick found on https://stackoverflow.com/questions/3143657/truncate-two-decimal-places-without-rounding
                    foreach (Course course in CoursesList)
                    {
                        course.FinalGrade = Math.Truncate(course.FinalGrade * 100)/100;
                    }

                    TB2Content.Visibility = Visibility.Collapsed;
                    CoursesStats.Visibility = Visibility.Visible;
                    CoursesStats.ItemsSource = CoursesList;

                    TB3Border.Visibility = Visibility.Collapsed; // hide TB3

                    NewNoteGrid.Background = (SolidColorBrush)Application.Current.Resources["orange"];
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading dynamic xaml into dialog: " + e);
            }
        }

        private void AddNotesToStudent()
        {
            try
            {
                List<Note> DBnotes = NotesRepository.GetStudentNotes((App.Current as App).ConnectionString, student.StudentID);

                //Was adding a blank note when in Advisor View for some reason -- this is to avoid that (find cause and fix later) 
                if (student.Notes.Count() > 0)
                {
                    student.Notes.Clear();
                }

                //Add notes from DB to Student's Notes list
                foreach (Note note in DBnotes)
                {
                    student.Notes.Add(note);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading student's notes: " + e);
            }
        }

        private void ContentDialog_ExitButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
          //If in advisor view, refresh table to remove "unread notes" message
          if (!MainPage.IsCurrentlyInstructor)
            {
                AdvisorViewUserControl.RefreshTable();
            }
        }

        private async void ContentDialog_SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (NoteContentBox.Text != "")
            {
                string noteContent = NoteContentBox.Text;
                DateTime date = DateTime.Now;
                                
                try
                {
                    //Create note "description" (like an email subject)
                    string noteDescription = "";
                    switch (StudentStatus)
                    {
                        case 1:
                            noteDescription = "Missing Assignments";
                            break;
                        case 2:
                            noteDescription = "Student Failing";
                            break;
                        case 3:
                            noteDescription = "Overall Danger";
                            break;
                        default:
                            noteDescription = "General Note";
                            break;
                    }

                    //Determine current role of user (instructor or advisor) and set noteType in Database
                    string userRole = "";
                    if (MainPage.IsCurrentlyInstructor)
                    {
                        userRole = "instructor";
                    }
                    else if (!MainPage.IsCurrentlyInstructor)
                    {
                        userRole = "advisor";
                    }

                    //Check for Private Note
                    bool isPrivate = false;
                    string privateMsg = "";
                    if (PrivateCheckBox.IsChecked == true)
                    {
                        isPrivate = true;
                        privateMsg = "Private";
                    }

                    //Actual databse call:
                    if (NotesRepository.InsertNotesInDatabase((App.Current as App).ConnectionString, student.StudentID, userRole, noteDescription, isPrivate, noteContent)) {
                        // if the note has been successfully inserted into the db, then Save note to Notes List so it shows in GUI
                        Note newNote = new Note(MainPage.User.FullName, date.ToString(), noteContent, isPrivate, privateMsg);
                        student.NoteAdded = true;
                        student.Notes.Insert(0, newNote);

                        if (MainPage.IsCurrentlyInstructor && MainPage.SpreadsheetInProgress)
                        {
                            SpreadsheetTableUserControl.RefreshTable();
                        }
                        else if (!MainPage.IsCurrentlyInstructor)
                        {
                            AdvisorViewUserControl.RefreshTable();
                        }
                    } 
                    else // if the insert fails
                    {
                        // This is code to keep from automatically closing the content dialog
                        var deferral = args.GetDeferral();
                        args.Cancel = true;
                        string noGood = "The note cannot be inserted into the database. \nPlease confirm the student ID "
                            + student.StudentID + " exists in the database.";

                        MessageDialog messageDialog = new MessageDialog(noGood, "Error");
                        await messageDialog.ShowAsync();
                        deferral.Complete();
                    }
                }
                catch (Exception eSql)
                {
                    MessageDialog messageDialog = new MessageDialog(eSql.ToString(), "Database Note Insert Error");
                    await messageDialog.ShowAsync();
                }
            } 
            else
            {
                // This is code to keep from automatically closing the content dialog
                var deferral = args.GetDeferral();
                args.Cancel = true;
                string noGood = "You can not save a blank note.";

                MessageDialog messageDialog = new MessageDialog(noGood, "Saving Blank Note");
                await messageDialog.ShowAsync();
                deferral.Complete();
            } 
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_DropDownClosed(object sender, object e)
        {
            try
            {
                if (comboBox.SelectedIndex >= 0)
                {
                    NoteContentBox.Text += comboBox.SelectedValue.ToString();
                    NoteContentBox.Text += ". ";
                }
            }
            catch (Exception e2)
            {
                Debug.WriteLine("Error with combo box: " + e2);
            }
        }

        private void NoteContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CoursesStats_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            Course course = CoursesList[args.ItemIndex];

            if (course.HasWithdrawn)
            {
                args.ItemContainer.Foreground = new SolidColorBrush(Colors.Gray);
            }            
            else if (course.WarningLvl == 1)
            {
                args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"];
            }
            else if (course.WarningLvl == 2)
            {
                args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-red"];
            }
            else if (course.WarningLvl > 2)
            {
                args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["striped"];
            }
            else
            {
                args.ItemContainer.Background = null;
            }            
        }
    }
}

