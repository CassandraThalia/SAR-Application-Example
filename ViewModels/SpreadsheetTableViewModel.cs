using SAR.ContentDialogs;
using SAR.Models;
using SAR.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace SAR.ViewModels
{
    public class SpreadsheetTableViewModel 
    {
        

        public static ObservableCollection<CSVParsedObject> CSVData { get; set; }
        public string HighestPossibleGrade { get; set; }
        public int AssessmentCheckIndex { get; set; }

        public async void DisplayTable(ObservableCollection<CSVParsedObject> _CSVData)
        {
            try
            {
                CSVData = _CSVData;
                foreach (var item in CSVData)
                {
                    double finalGrade = Double.Parse(item.Student.FinalGradeNumerator);
                    finalGrade = Math.Truncate(finalGrade * 100)/100;
                    item.Student.FinalGradeNumerator = finalGrade.ToString();
                }
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error initializing CSV Data: " + e);
                await errorMsg.ShowAsync();
            }

            try
            {
                CheckStudentStatus();
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error getting Student status warnings: " + e);
                await errorMsg.ShowAsync();
            }

            try
            {
                SpreadsheetTableUserControl stuc = new SpreadsheetTableUserControl(HighestPossibleGrade);
                MainPage.mainCC.Content = stuc;
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error displaying Spreadsheet Table User Control: " + e);
                await errorMsg.ShowAsync();
            }

        }

        public static double GetHightestGrade(ObservableCollection<CSVParsedObject> CSVData)
        {
            double highestFinal = 0;
            
            try
            {
                highestFinal =  double.Parse(CSVData[0].Student.FinalGradeNumerator);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error parsing highest final grade: " + e);
            }

            try
            {
                for (int i = 1; i < CSVData.Count; i++)
                {
                    double final = double.Parse(CSVData[i].Student.FinalGradeNumerator);

                    if (final > highestFinal)
                    {
                        highestFinal = final;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error obtaining highest grade in student list: " + e);
            }

            return highestFinal;
        }

        public void CheckStudentStatus()
        {
            //Final grade check
            double highestFinal = 0;
            try
            {
                highestFinal = double.Parse(HighestPossibleGrade);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error parsing highest possible grade from instructor input: " + e);
            }

            //Check if students' grade is less than 60% of the highest possible grade
            try
            {
                foreach (CSVParsedObject row in CSVData)
                {
                    double final = double.Parse(row.Student.FinalGradeNumerator);

                    if (final < (highestFinal * .60) && !row.Student.NoteAdded)
                    //if (final < 20)
                    {
                        row.StudentInDanger = true;
                    }
                    else
                    {
                        row.StudentInDanger = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for students in danger from student list: " + e);
            }

            //Check if student has submitted the selected assessment, if applicable
            try
            {
                foreach (CSVParsedObject row in CSVData)
                {
                    if (AssessmentCheckIndex >= 0)
                    {
                        row.AsssessmentResult = row.Student.Assessment[AssessmentCheckIndex].StudentEvaluationGrade;

                        if (double.Parse(row.Student.Assessment[AssessmentCheckIndex].StudentEvaluationGrade) == 0)
                        {
                            row.AssessmentNotSubmitted = true;
                        }
                        else
                        {
                            row.AssessmentNotSubmitted = false;
                        }
                    }
                    else
                    {
                        row.AsssessmentResult = "N/A";
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for students who have not submitted the selected assignment, if applicable: " + e);
            }
        }

        public static async void CreateEnrollmentIfNecessary(ObservableCollection<CSVParsedObject> CSVData)
        {
            // potentially return a message if update is not successful
            string errorStr = "";

            foreach (CSVParsedObject row in CSVData)
            {
                try
                {
                    bool isInserted = StudentRepository.CreateStudentEnrollment(row);
                    if (!isInserted)
                    {
                        errorStr += "Cannot insert enrollment for student ID: " + row.Student.StudentID + "\n";
                    }
                }
                catch (Exception eSql)
                {
                    Debug.WriteLine(eSql.ToString(), "Error saving final grades to database for student " + row.Student.StudentID);
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



        public static async void UpdateStudentFinalGradesWarningLvl(ObservableCollection<CSVParsedObject> CSVData)
        {
            // potentially return a message if update is not successful
            string errorStr = "";

            //Loop through each student and save their current Final Grade to the Database
            foreach (CSVParsedObject row in CSVData)
            {
                int warningLvl = 0;
                try
                {
                    if (row.StudentInDanger && row.AssessmentNotSubmitted)
                    {
                        warningLvl = 3;
                    }
                    else if (row.StudentInDanger && !row.AssessmentNotSubmitted)
                    {
                        warningLvl = 2;
                    }
                    else if (!row.StudentInDanger && row.AssessmentNotSubmitted)
                    {
                        warningLvl = 1;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error retrieving student warning level: " + e);
                }
                // Hard-coded false here for IsWithdrawing because we handle withdraws in the UnclearStudentsList file
                try
                {
                    bool isUpdated = StudentRepository.SaveFinalGradesToDB((App.Current as App).ConnectionString, row.Student.StudentID, row.CourseID, row.Student.FinalGradeNumerator, warningLvl, false);
                    if (!isUpdated)
                    {
                        errorStr += "Cannot update grade for student ID: " + row.Student.StudentID + "\n";
                    }
                }
                catch (Exception eSql)
                {
                    Debug.WriteLine(eSql.ToString(), "Error saving final grades to database for student " + row.Student.StudentID);
                }
            }
            // pop up a warning message if there is any unsuccessful update.
            if (errorStr != "")
            {
                errorStr += "\nPlease make sure the IDs exist in the database.";
                MessageDialog errorMsg = new MessageDialog(errorStr, "Warning");
                await errorMsg.ShowAsync();
            }
        }

        // This is a function to compare the Students being uploaded for the current course compared to the Database record of the current course.
        // The purpose of this is to improve tracking for students who may have registered late or have withdrawn from the course
        public async static void CompareStudentEnrollmentStatus()
        {

            List<Student> enrolledStudents = new List<Student>();
            // List to hold students who are not currently in the database
            List<CSVParsedObject> studentsInCSVNotDB = new List<CSVParsedObject>();
            // List to hold students who may have withdrawn
            List<CSVParsedObject> studentsInDBNotCSV = new List<CSVParsedObject>();
            //List to hold students who are not yet enrolled in the uploaded course
            List<CSVParsedObject> studentsNotEnrolled = new List<CSVParsedObject>();

            // First, check for students who are not in the DB at all
            try
            {
                List<Student> allRegisteredStudents = StudentRepository.GetAllRegisteredStudents();
                List<string> studentIDs = new List<string>();
                foreach (var student in allRegisteredStudents)
                {
                    studentIDs.Add(student.StudentID);
                }
                foreach (var record in CSVData)
                {
                    if (!studentIDs.Contains(record.Student.StudentID))
                    {
                        studentsInCSVNotDB.Add(record);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for registered students in database: " + e);
            }

            // Then check for students who are not enrolled in this specific course
            List<string> courseCodes = new List<string>();

            try
            {
                // Get all of the course codes contained within the CSV
                // This will generally be only one code, but we have to account for edge cases
                foreach (var record in CSVData)
                {
                    // If the current record's coursecode is not within the list
                    if (!courseCodes.Contains(record.CourseID))
                    {
                        courseCodes.Add(record.CourseID);
                    }
                }

                // For all of the course codes within the spreadsheet
                foreach (var courseCode in courseCodes)
                {
                    // First get all the students associated with that course in the DB
                    enrolledStudents = StudentRepository.GetStudentsByCourse(courseCode);

                    // Get a list of all student numbers pulled from DB
                    List<string> wNumsOfEnrolledStudents = new List<string>();
                    foreach (var student in enrolledStudents)
                    {
                        wNumsOfEnrolledStudents.Add(student.StudentID);
                    }

                    // Get a list of all student numbers in the CSV file
                    List<string> wNumsOfStudentsInCSV = new List<string>();
                    // Loop through all students in csvData and add to list not in DB if applicable
                    foreach (var record in CSVData)
                    {   // While looping, add student ID to new list
                        wNumsOfStudentsInCSV.Add(record.Student.StudentID);

                        if (record.CourseID == courseCode)
                        {
                            //Also check if the record has already been added to the studentsInCSVNotDB list to avoid duplicates
                            if (!wNumsOfEnrolledStudents.Contains(record.Student.StudentID) && !studentsInCSVNotDB.Contains(record))
                            {
                                studentsNotEnrolled.Add(record);
                            }
                        }
                    }

                    // Then, loop through students in DB to find list of students in DB but not in CSV for possible withdrawals
                    foreach (var student in enrolledStudents)
                    {
                        if (!wNumsOfStudentsInCSV.Contains(student.StudentID))
                        {
                            CSVParsedObject newObj = new CSVParsedObject();
                            newObj.CourseID = courseCode;
                            newObj.Student = student;
                            studentsInDBNotCSV.Add(newObj);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for enrolled students: " + e);
            }

            // If there are any mismatched entries, bring up the dialog box
            if (studentsInCSVNotDB.Count > 0 || studentsInDBNotCSV.Count > 0 || studentsNotEnrolled.Count > 0)
            {
                try
                {
                    UnclearStudentStatusList statusBox = new UnclearStudentStatusList(studentsInCSVNotDB, studentsInDBNotCSV, studentsNotEnrolled);
                    await statusBox.ShowAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error launching Unclear Students dialog: " + e);
                }
            }
        }
    }
}
