using SAR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Repository
{
    public class InstructorRepository
    {
        public static void GetInstructorInfo(Instructor instructor)
        {
            string GetDataQuery = "EXEC sp_getInstructorInfo '" + instructor.WNumber + "'";

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
                                    instructor.FullName = reader.GetString(1);
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
    }
}
