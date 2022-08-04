using SAR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Repository
{
    public class AssessmentsRepository
    {
        public List<Assessment> GetAllAssessmentsForStudent(Student student)
        {
            List<Assessment> assessments = new List<Assessment>();
            return assessments;
        }
    }
}
