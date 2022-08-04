using SAR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SAR.Repository
{
    public class NotesRepository
    {
        //Get all notes related to a student from the Database (all public notes & private notes for logged-in faculty user)
        public static List<Note> GetStudentNotes(string connectionString, string studentID)
        {
            List<Note> notes = new List<Note>();

            //Stored Procedure: getStudentNotes
            string GetDataQuery = "EXEC sp_getStudentNotes '" + MainPage.User.WNumber + "', '" + studentID + "'";
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
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string noteDate = reader.GetDateTime(8).ToString();

                                    Note note = new Note();
                                    note.Author = reader.GetString(4) + " " + reader.GetString(5);
                                    note.DateString = noteDate;
                                    note.Date = reader.GetDateTime(8);
                                    note.Content = reader.GetString(6);
                                    note.IsPrivate = reader.GetBoolean(7);

                                    if (note.IsPrivate)
                                    {
                                        note.PrivateMsg = "Private";
                                    }
                                    else if (!note.IsPrivate)
                                    {
                                        note.PrivateMsg = "";
                                    }

                                    notes.Add(note);
                                }
                            };
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return notes;
        }

        public static bool InsertNotesInDatabase(string connectionString, string studentID, string noteType, string noteHeader, bool isPrivate, string noteContent)
        {
            //NoteType means Instructor or Advisor note
            string GetDataQuery = "EXEC sp_insertNewNote '" + studentID + "', '" + noteType + "', '" + MainPage.User.WNumber + "', '" + noteHeader + "', '" + isPrivate + "', '" + noteContent + "'";
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
