using SAR.Models;
using SAR.Repository;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SAR.ContentDialogs
{
    public sealed partial class UnclearStudentStatusList : ContentDialog
    {
        public ObservableCollection<CSVParsedObject> StudentsInCSVNotDB;
        public ObservableCollection<CSVParsedObject> StudentsInDBNotCSV;
        public ObservableCollection<CSVParsedObject> StudentsNotEnrolled;
        //private bool WithdrawStudent = false;
        public static string CourseCode { get; set; }
        // Constructor takes two parameters, the collection of students
        // and the status code determining if these are withdrawn or late enrollment students
        public UnclearStudentStatusList(List<CSVParsedObject> inCSVNotDB, List<CSVParsedObject> inDBNotCSV, List<CSVParsedObject> notEnrolled)
        {
            this.InitializeComponent();

            StudentsInCSVNotDB = new ObservableCollection<CSVParsedObject>();
            
            foreach (var record in inCSVNotDB)
            {
                StudentsInCSVNotDB.Add(record);
            }
            StudentsInDBNotCSV = new ObservableCollection<CSVParsedObject>();
            foreach (var record in inDBNotCSV)
            {
                StudentsInDBNotCSV.Add(record);
            }
            StudentsNotEnrolled = new ObservableCollection<CSVParsedObject>();
            foreach (var record in notEnrolled)
            {
                StudentsNotEnrolled.Add(record);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // potentially return a message if update is not successful
            string errorStr = "";

            //Check for withdrawing students and send info to Database
            foreach (var record in StudentsInDBNotCSV)
            {
                if (record.Student.IsWithdrawing)
                {
                    try
                    {
                        bool isUpdated = StudentRepository.SaveFinalGradesToDB((App.Current as App).ConnectionString, record.Student.StudentID, record.CourseID, "0", 0, record.Student.IsWithdrawing);
                        if (!isUpdated)
                        {
                            errorStr += "Cannot update withdrawl status for student ID: " + record.Student.StudentID + "\n";
                        }
                    }
                    catch (Exception e2)
                    {
                        Debug.WriteLine("Error withdrawing student: " + e2);
                    }
                }
            }

            // pop up a warning message if there is any unsuccessful update.
            if (errorStr != "")
            {
                errorStr += "\nPlease contact the system administrator.";
                MessageDialog errorMsg = new MessageDialog(errorStr, "Warning");
                await errorMsg.ShowAsync();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LateEnrollButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WithdrawnButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // For now, hard coding 0 as the final grade. In future, create a Stored Procedure that just updates the Withdrawn bool so we can maintain the old final grade
        private void WithdrawnCheck_Checked(object sender, RoutedEventArgs e)
        {
            CSVParsedObject data = (CSVParsedObject)(sender as FrameworkElement).DataContext;
            Student studentOfNote = data.Student;
            studentOfNote.IsWithdrawing = true;
        }

        private void WithdrawnCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            CSVParsedObject data = (CSVParsedObject)(sender as FrameworkElement).DataContext;
            Student studentOfNote = data.Student;
            studentOfNote.IsWithdrawing = false;
        }
    }
}
