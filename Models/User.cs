using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
    public class User
    {
        //public int Id { get; set; }
        public string WNumber { get; set; }
        public string FullName { get; set; }
        public bool IsChair { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Role { get; set; }
        public DateTime LastLogin { get; set; }

        //public User(string wNum, string fName, string lName, string role)

        public User(string wNum)
        {
            WNumber = wNum;
        }
        public User(string wNum, string name)
        {
            WNumber = wNum;
            FullName = name;
            //Role = role;
            //Courses = GetUserCourses(wNum);
        }


        // Method for retrieving instructor courses
        // TODO: Flesh out this method once fuller understanding of DB structure is attained
        //public List<Course> GetUserCourses(string wNum)
        //{
        //    return Courses;
        //}


    }
}

