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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SAR
{
    public sealed partial class SpreadsheetUploadSelectorDialog : ContentDialog
    {
        //ObservableCollection<JSONObject> JSONData = new ObservableCollection<JSONObject>();
        ObservableCollection<CSVParsedObject> CSVData = new ObservableCollection<CSVParsedObject>();

        //public SpreadsheetUploadSelectorDialog(ObservableCollection<JSONObject> _JSONData)
        public SpreadsheetUploadSelectorDialog(ObservableCollection<CSVParsedObject> _JSONData)
        {
            this.InitializeComponent();
            //JSONData = _JSONData;
            CSVData = _JSONData;

            //string hightestGrade = SpreadsheetTableViewModel.GetHightestGrade(JSONData).ToString();

            //FileName.Text = JSONData[0].FileName;
            //ExportDate.Text = JSONData[0].Date;
            //CourseName.Text = JSONData[0].CourseName;
            //InstructorName.Text = JSONData[0].InstructorFName + " " + JSONData[0].InstructorLName;
            //HighestPossibleGrade.Text = hightestGrade;


            //for (int j = 0; j < JSONData[0].Student.Assessment.Count; j++)
            //{
            //    AssessmentChecker.Items.Add(JSONData[0].Student.Assessment[j].AssessmentName);
            //}

            string hightestGrade = SpreadsheetTableViewModel.GetHightestGrade(CSVData).ToString();

            try
            {
                FileName.Text = CSVData[0].FileName;
                ExportDate.Text = CSVData[0].Date;
                CourseName.Text = CSVData[0].CourseName;
                InstructorName.Text = CSVData[0].InstructorFName + " " + CSVData[0].InstructorLName;
                HighestPossibleGrade.Text = hightestGrade;


                for (int j = 0; j < CSVData[0].Student.Assessment.Count; j++)
                {
                    AssessmentChecker.Items.Add(CSVData[0].Student.Assessment[j].AssessmentName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting CSV info: " + e);
            }

            // Use existing stored procedures to check for the last time this course was updated in the database
            // (Currently going through the Student model, but in the future would be best to have it tied directly to the course)
            try
            {
                // Get all of the course codes contained within the CSV to account for spreadsheets with multiple courses
                List<string> courseCodes = new List<string>();
                string courseCodesStr = "";
                foreach (var record in CSVData)
                {
                    // If the current record's coursecode is not within the list, add it to the list of courses and the output message
                    if (!courseCodes.Contains(record.CourseID))
                    {
                        courseCodes.Add(record.CourseID);
                        courseCodesStr += record.CourseID;
                        courseCodesStr += " ";
                    }
                }
                //If there is only one course in the spreadsheet...
                if (courseCodes.Count == 1)
                {
                    //Get a sample student from the course
                    Student student = StudentRepository.GetOneStudentByCourse(CSVData[0].CourseID);
                    if (student != null)
                    {
                        //Get the last upload date for this student for this course
                        string lastUpdate = StudentRepository.GetLastCourseUpdateByStudent(student, CSVData[0].CourseID);
                        string currentDate = DateTime.Now.ToShortDateString();
                        if (lastUpdate != null)
                        {
                            //If the dates are the same, then a spreadsheet has already been uploaded for this course today. Display message.
                            if (lastUpdate == currentDate)
                            {
                                DateWarning.Text = "Warning! You have already uploaded a spreadsheet for course " + CSVData[0].CourseName + ", section " + CSVData[0].CourseID + " today.";
                                DateWarning.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                //If there is more than one course in the spreadsheet...
                else if (courseCodes.Count > 1)
                {
                    List<bool> dateMatches = new List<bool>();
                    //Loop through each course and repeat the procedure above
                    foreach (string courseID in courseCodes)
                    {
                        Student student = StudentRepository.GetOneStudentByCourse(courseID);
                        if (student != null)
                        {
                            string lastUpdate = StudentRepository.GetLastCourseUpdateByStudent(student, courseID);
                            string currentDate = DateTime.Now.ToShortDateString();
                            if (lastUpdate != null)
                            {
                                //If dates match, change dateMatches bool
                                if (lastUpdate == currentDate)
                                {
                                    dateMatches.Add(true);
                                }
                                else
                                {
                                    dateMatches.Add(false);
                                }
                            }
                        }
                    }
                    if (!dateMatches.Contains(false))
                    {
                        DateWarning.Text = "Warning! You have already uploaded a spreadsheet today for course " + CSVData[0].CourseName + ", sections " + courseCodesStr + ".";
                        DateWarning.Visibility = Visibility.Visible;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Error checking date of last upload: " + e);
            }
        }

        //Cancel button
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        //Show table button
        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                SpreadsheetTableViewModel stvm = new SpreadsheetTableViewModel();

                string highestGrade = HighestPossibleGrade.Text;
                stvm.HighestPossibleGrade = highestGrade;

                stvm.AssessmentCheckIndex = AssessmentChecker.SelectedIndex;

                //Display spreadsheet
                //stvm.DisplayTable(JSONData);
                stvm.DisplayTable(CSVData);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error displaying spreadsheet: " + e);
            }
        }
    }
}
