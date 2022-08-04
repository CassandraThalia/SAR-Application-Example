using SAR.ContentDialogs;
using SAR.Models;
using SAR.Repository;
using SAR.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SAR
{
    public sealed partial class AdvisorViewUserControl : UserControl
    {
        private static ObservableCollection<Student> Advisees = new ObservableCollection<Student>();
        public static ListView AdviseesList { get; set; }
        public StackPanel colorLegend = new StackPanel();
        public AdvisorViewUserControl()
        {
            this.InitializeComponent();

            try
            {
                Advisees = Repository.StudentRepository.GetAdvisees(MainPage.User);
                SetButton();

                AdviseesList = AdvisorView;
                AdviseesList.ItemsSource = Advisees;


                GetAllAdviseesCourseStats();

                //Get timestamp for last time user was in Advisor View
                Repository.UserRepository.GetLastLogin(MainPage.User);
                //Update the same timestamp so it reflects the current time
                Repository.UserRepository.UpdateLastLoginTimestamp(MainPage.User);
                //Check for unread notes
                StudentViewModel.CheckForUnreadNotes(Advisees);
                //Reorder the list of advisees so that students with unread notes are at the top
                Advisees = ReorderAdvisees();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting Advisees and info: " + e);
            }

            try
            {
                //Add legend to Main Page Side Bar
                // *** Note about background colors: there is no reason for the first two to be gradients, but SolidColorBrush was causing weird hover behavior.
                // *** Changing them to LinearGradientBrush fixed the problem, for some reason.
                StackPanel redPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["str-red"] };
                redPanel.Padding = new Thickness(5);
                redPanel.Margin = new Thickness(10, 30, 10, 10);
                redPanel.Width = 220;
                redPanel.CornerRadius = new CornerRadius(3);
                TextBlock redColor = new TextBlock { Text = "Student is failing one of their courses" };
                redColor.TextWrapping = TextWrapping.Wrap;
                redColor.TextAlignment = TextAlignment.Center;
                redColor.FontSize = 12;
                redColor.Foreground = (SolidColorBrush)Application.Current.Resources["black"];
                redPanel.Children.Add(redColor);
                colorLegend.Children.Add(redPanel);

                StackPanel yellowPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"] };
                yellowPanel.Padding = new Thickness(5);
                yellowPanel.Margin = new Thickness(10, 0, 10, 10);
                yellowPanel.Width = 220;
                yellowPanel.CornerRadius = new CornerRadius(3);
                TextBlock yellowColor = new TextBlock { Text = "Student did not submit one of their most recent assessments" };
                yellowColor.TextWrapping = TextWrapping.Wrap;
                yellowColor.TextAlignment = TextAlignment.Center;
                yellowColor.FontSize = 12;
                yellowPanel.Children.Add(yellowColor);
                colorLegend.Children.Add(yellowPanel);

                StackPanel stripedPanel = new StackPanel { Background = (LinearGradientBrush)Application.Current.Resources["striped"] };
                stripedPanel.Padding = new Thickness(5);
                stripedPanel.Margin = new Thickness(10, 0, 10, 10);
                stripedPanel.Width = 220;
                stripedPanel.CornerRadius = new CornerRadius(3);
                TextBlock stirpedColor = new TextBlock { Text = "Both warnings are applicable" };
                stirpedColor.TextWrapping = TextWrapping.Wrap;
                stirpedColor.TextAlignment = TextAlignment.Center;
                stirpedColor.FontSize = 12;
                stripedPanel.Children.Add(stirpedColor);
                colorLegend.Children.Add(stripedPanel);

                MainPage.SideBar.Children.Add(colorLegend);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error adding legend and save button to side bar: " + e);
            }

        }

        private async void ViewNotesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Student studentOfNote = (Student)(sender as FrameworkElement).DataContext;

                if (studentOfNote.FailingWarning && studentOfNote.MissingAssessmentWarning)
                {
                    StudentDetailsDialog stdntDialog = new StudentDetailsDialog(studentOfNote, 3);
                    await stdntDialog.ShowAsync();
                }
                else if (studentOfNote.MissingAssessmentWarning)
                {
                    StudentDetailsDialog stdntDialog = new StudentDetailsDialog(studentOfNote, 1);
                    await stdntDialog.ShowAsync();
                }
                else if (studentOfNote.FailingWarning)
                {
                    StudentDetailsDialog stdntDialog = new StudentDetailsDialog(studentOfNote, 2);
                    await stdntDialog.ShowAsync();
                }
                else
                {
                    StudentDetailsDialog stdntDialog = new StudentDetailsDialog(studentOfNote, 0);
                    await stdntDialog.ShowAsync();
                }
            }
            catch
            {
                MessageDialog errorMsg = new MessageDialog("Error loading Student Details Dialog: " + e);
                await errorMsg.ShowAsync();
            }
        }

        private async void GetAllAdviseesCourseStats()
        {
            try
            {
                foreach (Student student in Advisees)
                {
                    student.CoursesGrades = Repository.StudentRepository.GetCoursesStats(student);
                    AdviseeFailing(student);
                    AdviseeMissingAssignments(student);
                }
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error retrieving course stats for advisees: " + e);
                await errorMsg.ShowAsync();
            }
        }

        private void AdviseeFailing(Student student)
        {
            int failingCounter = 0;

            try
            {
                foreach (Course courseStat in student.CoursesGrades)
                {
                    if (courseStat.WarningLvl >= 2)
                    {
                        failingCounter++;
                    }
                }
                if (failingCounter > 0)
                {
                    student.FailingWarning = true;
                }
                else
                {
                    student.FailingWarning = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for student failing: " + e);
            }
        }

        private void AdviseeMissingAssignments(Student student)
        {
            int missingCounter = 0;
            try
            {
                foreach (Course courseStat in student.CoursesGrades)
                {
                    if (courseStat.WarningLvl == 1 || courseStat.WarningLvl == 3)
                    {
                        missingCounter++;
                    }
                }
                if (missingCounter > 0)
                {
                    student.MissingAssessmentWarning = true;
                }
                else
                {
                    student.MissingAssessmentWarning = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting student assessment warning status: " + e);
            }
        }

        private void SetButton()
        {
            foreach (Student student in Advisees)
            {
                student.ButtonBG = (SolidColorBrush)Application.Current.Resources["NSCCblue"];
                student.ButtonFG = (SolidColorBrush)Application.Current.Resources["white"];
                student.ButtonMsg = "View Notes";
            }
        }

        // This function sets the background color of the listview row based on student's courses warning level
        private void AdvisorView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            try
            {
                Student advisee = Advisees[args.ItemIndex];
                string noteSaved = "Note Saved!";

                if (advisee.FailingWarning && advisee.MissingAssessmentWarning && !advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["striped"];
                }
                else if (advisee.FailingWarning && advisee.MissingAssessmentWarning && advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["striped"];
                    advisee.UnreadNotes = "";
                    advisee.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    advisee.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    advisee.ButtonMsg = noteSaved;
                }
                else if (advisee.FailingWarning && !advisee.MissingAssessmentWarning && !advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-red"];
                }
                else if (advisee.FailingWarning && !advisee.MissingAssessmentWarning && advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-red"];
                    advisee.UnreadNotes = "";
                    advisee.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    advisee.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    advisee.ButtonMsg = noteSaved;
                }
                else if (advisee.MissingAssessmentWarning && !advisee.FailingWarning && !advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"];
                }
                else if (advisee.MissingAssessmentWarning && !advisee.FailingWarning && advisee.NoteAdded)
                {
                    args.ItemContainer.Background = (LinearGradientBrush)Application.Current.Resources["str-yellow"];
                    advisee.UnreadNotes = "";
                    advisee.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    advisee.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    advisee.ButtonMsg = noteSaved;
                }
                else if (!advisee.MissingAssessmentWarning && !advisee.FailingWarning && advisee.NoteAdded)
                {
                    advisee.UnreadNotes = "";
                    advisee.ButtonFG = (SolidColorBrush)Application.Current.Resources["black"];
                    advisee.ButtonBG = (SolidColorBrush)Application.Current.Resources["gray"];
                    advisee.ButtonMsg = noteSaved;
                }
                else
                {
                    args.ItemContainer.Background = null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error changing table row color: " + e);
            }
        }

        public static async void RefreshTable()
        {
            // Inspiration for this is from here: https://stackoverflow.com/questions/49370365/how-to-refresh-a-user-control-every-time-a-button-is-clicked
            try
            {
                AdviseesList.ItemsSource = null;
                AdviseesList.ItemsSource = Advisees;
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error refreshing advisor table: " + e);
                await errorMsg.ShowAsync();
            }
        }

        public static ObservableCollection<Student> ReorderAdvisees()
        {
            ObservableCollection<Student> adviseesSorted = Advisees;
            try
            {
                int nextSpace = 0;
                for (int i = 0; i < adviseesSorted.Count(); i++)
                {
                    if (adviseesSorted[i].UnreadNotes != "")
                    {
                        adviseesSorted.Move(i, nextSpace);
                        nextSpace++;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reordering list of advisees: " + e);
            }
            return adviseesSorted;
        }
    }
}
