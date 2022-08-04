using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
    public class Instructor
    {
        public string WNumber { get; set; }

        public string FullName { get; set; }

        public Instructor(string wNum, string fName, string lName)
        {
            WNumber = wNum;
            FullName = fName + " " + lName;
        }

        public Instructor(string wNum)
        {
            WNumber = wNum;
        }
    }
}
