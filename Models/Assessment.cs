using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
    public class Assessment
    {
        public string AssessmentName { get; set; }
        public string StudentEvaluationGrade { get; set; }
        public string AssessmentDeadline { get; set; }
        public Evaluation Evaluation { get; set; }

        public Assessment(string assessmentName, string studentEvaluationGrade, string assessmentDeadline)
        {
            AssessmentName = assessmentName;
            StudentEvaluationGrade = studentEvaluationGrade;
            AssessmentDeadline = assessmentDeadline;
        }

        public Assessment()
        {
            Evaluation = new Evaluation();
        }




        //// member variables based on the column name of the csv file
        //public string CourseID { get; } 
        //public string Name { get; }
        //public double CurrGrade { get; set; }
        //public string Category { get; }        
        //public double MaxPoints { get; }
        //public double Weight { get; }
        //public double CategoryWeight { get; }

        //// constructor
        //public Assessment(string courseId, string name, double currGrade, string category, double maxpnt, double weight, double catWeight)
        //{
        //    CourseID = courseId;
        //    Name = name;
        //    CurrGrade = currGrade;
        //    Category = category;
        //    MaxPoints = maxpnt;
        //    Weight = weight;
        //    CategoryWeight = catWeight;
        //}
    }
}
