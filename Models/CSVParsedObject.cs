using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SAR.Models
{
	/// <summary>
	/// This is a class built on modelling the information we get out of the CSV Files upon upload.
	/// This is intended to be portable with the JSON Object.
	/// This is handled within the CSV Parsing Service
	/// </summary>
    public class CSVParsedObject
    {
		public string FileName { get; set; }
		public string Date { get; set; }
		public string Time { get; set; }
		public string ProgramID { get; set; }
		public string ProgramName { get; set; }
		public string ProgramDuration { get; set; }
		public string CourseID { get; set; }
		public string CourseName { get; set; }
		public string CourseDescription { get; set; }
		public string InstructorFName { get; set; }
		public string InstructorLName { get; set; }
		public Student Student { get; set; }
		public bool StudentInDanger { get; set; }
		public bool AssessmentNotSubmitted { get; set; }
		public string AsssessmentResult { get; set; }

		public CSVParsedObject()
        {
			Student = new Student();
			Student.Assessment = new List<Assessment>();
		}
	}
}
