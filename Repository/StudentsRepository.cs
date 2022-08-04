using SAR.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Repository
{
    // Class for handling application methods related to retrieving students
    public class StudentRepository
    {
        // only gets advisees, for Advisor View
        public static ObservableCollection<Student> GetAdvisees(User user)
        {
            string GetDataQuery = "";

            if (!user.IsChair)
            {
                GetDataQuery = "EXEC sp_getAdvisorAdvisees '" + user.WNumber + "'";
            }
            else
            {
                GetDataQuery = "EXEC sp_getAllPublicStudentsNotes";
            }

            var advisees = new ObservableCollection<Student>();

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var stu = new Student();
                                    stu.StudentID = reader.GetString(0);
                                    stu.StudentFName = reader.GetString(1);
                                    stu.StudentLName = reader.GetString(2);
                                    GetStudentAdvisor(stu);

                                    advisees.Add(stu);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return advisees;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        // gets all students related to the user, for searching feature
        public static List<Student> GetStudentsByUser(User instructor)
        {
            List<Student> students = new List<Student>();
            string GetDataQuery = "";

            if (!instructor.IsChair)
            {
                GetDataQuery = "EXEC sp_searchMenuLogin '" + MainPage.User.WNumber + "'";
            } else
            {
                GetDataQuery = "EXEC sp_searchMenuLoginAcademicChair";
            }           
            
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var stu = new Student();
                                    stu.StudentID = reader.GetString(0);
                                    stu.StudentFName = reader.GetString(1);
                                    stu.StudentLName = reader.GetString(2);


                                    students.Add(stu);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return students;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;            
        }



        public static List<Student> GetStudentsByCourse(string courseId)
        {
            List<Student> students = new List<Student>();
            string GetDataQuery = "";

            
            GetDataQuery = "EXEC sp_getAllStudentsEnrolledCourse '" + courseId + "'";
            

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var stu = new Student();
                                    // First check if the "StudentHasWithdrawn" boolean is set to True (if so, do not add student to enrolled list)
                                    if (!reader.GetBoolean(7))
                                    {
                                        stu.StudentID = reader.GetString(0);
                                        stu.StudentFName = reader.GetString(1);
                                        stu.StudentLName = reader.GetString(2);
                                        students.Add(stu);
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return students;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static Student GetOneStudentByCourse(string courseId)
        {
            Student student = new Student();
            string GetDataQuery = "";


            GetDataQuery = "EXEC sp_getAllStudentsEnrolledCourse '" + courseId + "'";


            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    // First check if the "StudentHasWithdrawn" boolean is set to True (if so, do not add student to enrolled list)
                                    if (!reader.GetBoolean(7))
                                    {
                                        student.StudentID = reader.GetString(0);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return student;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        //Final parameter is "has withdrawn", currently set to always be false
        public static bool SaveFinalGradesToDB(string connectionString, string studentID, string courseCode, string grade, int warningLvl, bool isWithdrawing)
        {
            string GetDataQuery = "EXEC sp_updateStudentFinalGradeWarningLvl '" + studentID + "', '" + courseCode + "', " + grade + ", " + warningLvl + ", '" + isWithdrawing + "'";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                // when 0 row is affected, it means update failed.
                                conn.Close();
                                return false;
                            }
                        }
                    }
                    conn.Close();
                    return true;
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;
            }
        }

        public static List<Student> GetAllRegisteredStudents()
        {
            string GetDataQuery = "EXEC sp_getAllRegisteredStudents";
            List<Student> allRegisteredStudents = new List<Student>();
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var stu = new Student();
                                    stu.StudentID = reader.GetString(0);
                                    allRegisteredStudents.Add(stu);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return allRegisteredStudents;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static List<Course> GetCoursesStats(Student stu)
        {
            var courses = new List<Course>();

            string GetDataQuery = "EXEC sp_getStudentAcademicInfoWarning '" + stu.StudentID + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var course = new Course();
                                    course.CourseID = reader.GetString(4);
                                    course.CourseName = reader.GetString(5);
                                    course.FinalGrade = reader.GetDouble(6);
                                    if (!reader.IsDBNull(7))
                                    {
                                        course.WarningLvl = reader.GetInt32(7);
                                    }
                                    course.HasWithdrawn = reader.GetBoolean(8);

                                    if (!reader.IsDBNull(9))
                                    {
                                        course.DateOfStats = reader.GetDateTime(9).ToShortDateString();
                                    } else
                                    {
                                        course.DateOfStats = "";
                                    }                                    

                                    courses.Add(course);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return courses;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static string GetLastCourseUpdateByStudent(Student stu, string courseID)
        {
            string GetDataQuery = "EXEC sp_getStudentAcademicInfoWarning '" + stu.StudentID + "'";
            string lastCourseUpdate = "";
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(9) && reader.GetString(4) == courseID) 
                                    {
                                        lastCourseUpdate = reader.GetDateTime(9).ToShortDateString();
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                return lastCourseUpdate;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

            public static void GetStudentAdvisor(Student stu)
        {
            string GetDataQuery = "EXEC sp_getStudentAdvisor " + "'" + stu.StudentID + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    stu.Advisor = new Instructor(reader.GetString(1));
                                    // updates the advisor object with name
                                    Repository.InstructorRepository.GetInstructorInfo(stu.Advisor);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
        }

        public static string GetStudentYear(Student stu)
        {
            string GetDataQuery = "EXEC sp_getStudentAcademicInfo " + "'" + stu.StudentID + "'";
            int year = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    year = reader.GetInt32(3);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return year.ToString();
        }

        // This function interacts with the student_course table. It inserts a new relationship between student and course if one does not already exist.
        //public static void CreateStudentEnrollment(JSONObject data)
        public static bool CreateStudentEnrollment(CSVParsedObject data)
        {
            string GetDataQuery = "EXEC sp_insertNewEnrollment " + "'" + data.Student.StudentID + "', '" + data.CourseID + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                // when 0 row is affected, it means insert failed.
                                conn.Close();
                                return false;
                            }
                        }
                    }
                    conn.Close();
                    return true;
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                return false;
            }
        }
    }
}
