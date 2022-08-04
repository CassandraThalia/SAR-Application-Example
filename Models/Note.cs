using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAR.Models
{
    public class Note
    {
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public string Content { get; set; }
        public string CourseCode { get; set; }
        public bool IsPrivate { get; set; }
        public string PrivateMsg { get; set; }

        public Note() {  }

        public Note(string author, string date, string content, bool isPrivate, string privateMsg)
        {
            Author = author;
            DateString = date;
            Content = content;
            IsPrivate = isPrivate;
            PrivateMsg = privateMsg;
        }
    }
}
