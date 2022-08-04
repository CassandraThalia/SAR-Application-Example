using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace SAR.Models
{
    public class Student
    {
        public string StudentID { get; set; }
        public string StudentFName { get; set; }
        public string StudentLName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentRegDate { get; set; }
        public string FinalGradeNumerator { get; set; }
        //public string FinalGradeDenominator { get; set; }
        public List<Assessment> Assessment { get; set; }
        public List<Note> Notes = new List<Note>();
        public bool NoteAdded { get; set; }

        // new member vars
        public Instructor Advisor { get; set; }
        // needs a course list as a member var with grades info inside
        public List<Course> CoursesGrades = new List<Course>();

        public bool NotesLoaded { get; set; }

        public string UnreadNotes { get; set; }

        public bool FailingWarning { get; set; }
        public bool MissingAssessmentWarning { get; set; }
        public SolidColorBrush ButtonBG { get; set; }
        public SolidColorBrush ButtonFG { get; set; }
        public string ButtonMsg { get; set; }

        public bool IsWithdrawing { get; set; }

        //// Student Member variables
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string W_Number { get; set; }
        //public double CalculatedFinalGrade { get; set; }
        //public bool isInDanger { get; set; }
        //public bool hasNotSubmittedLastAssignment { get; set; }
        //public List<Assessment> _assessments = new List<Assessment>();
        //public List<Note> _notes = new List<Note>();

        //// Parameterized Constructor
        //public Student(string fName, string lName, string wNum, double calcGrade)
        //{
        //    FirstName = fName;
        //    LastName = lName;
        //    W_Number = wNum;
        //    CalculatedFinalGrade = calcGrade;
        //}

        //// Student Methods
        //// Methods for adding assessments
        //public void AddAssessment(Assessment assessment)
        //{
        //    _assessments.Add(assessment);
        //}

        //public void AddAssessments(List<Assessment> assessments)
        //{
        //    assessments.ForEach(assessment => _assessments.Add(assessment));
        //}

        //// Method for Assessment retrieval
        //public List<Assessment> GetAllAssessments()
        //{
        //    return _assessments;
        //}
        //public Assessment GetAssessmentByName(string name)
        //{
        //    return _assessments.FirstOrDefault(assessment => assessment.Name == name);
        //}

        //// Methods for note retrieval
        //public List<Note> GetAllNotes()
        //{
        //    return _notes;
        //}

        //public List<Note> GetNotesByAuthor(string author)
        //{
        //    List<Note> noteResults = new List<Note>();
        //    noteResults = _notes.FindAll(note => note.Author == author);
        //    return noteResults;
        //}
    }
}
