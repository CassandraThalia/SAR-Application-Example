using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
	public class JSONObject
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

	}
}
