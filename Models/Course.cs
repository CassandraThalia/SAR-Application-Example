using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
    public class Course
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public double FinalGrade { get; set; }

        // Codes current are: 0 - All fine; 1 - Assignment Not Submitted; 2 - Failing; 3 - Failing & Assignment Not Submitted
        public int WarningLvl { get; set; }
        public string DateOfStats { get; set; }
        public bool HasWithdrawn { get; set; }
        public string StatDescription
        { 
            get
            {
                if (HasWithdrawn)
                {
                    return "Withdrawn";
                } 
                else if (WarningLvl == 1)
                {
                    return "Missing assignment";
                } else if (WarningLvl == 2)
                {
                    return "At Risk of Failing";
                } else if (WarningLvl > 2)
                {
                    return "Risk of Failing and missing assignment";
                } 
                else { return ""; }
            }
        }

        public Course() { }
        public Course(string cName)
        {
            CourseName = cName;
        }

        public Course(string cID, string cName, double grade)
        {
            CourseID = cID;
            CourseName = cName;
            FinalGrade = grade;
        }
    }
}
