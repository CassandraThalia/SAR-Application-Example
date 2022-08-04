using SAR.Models;
using SAR.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SAR.ViewModels
{
    public class StudentViewModel
    {
        
        public static string UnreadMessage { get; set; }

        //Have string for note message here which can change when the user view the notes

        public static async void CheckForUnreadNotes(ObservableCollection<Student> Advisees)
        {
            try
            {
                if (Advisees != null)
                {
                    foreach (Student stu in Advisees)
                    {
                        List<Note> unreadNotes = new List<Note>();
                        stu.Notes = NotesRepository.GetStudentNotes((App.Current as App).ConnectionString, stu.StudentID);
                        foreach (Note note in stu.Notes)
                        {
                            if (note.Date > MainPage.User.LastLogin && note.Author != MainPage.User.FullName && !note.IsPrivate)
                            {
                                unreadNotes.Add(note);
                            }
                        }
                        int totalUnreadNotes = unreadNotes.Count();
                        if (totalUnreadNotes > 0)
                        {
                            stu.UnreadNotes = totalUnreadNotes.ToString() + " Unread Note(s)";
                        }
                        else
                        {
                            stu.UnreadNotes = "";
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                MessageDialog errorMsg = new MessageDialog("Error checking for unread notes: " + e);
                await errorMsg.ShowAsync();
            }
        }
    }
}
