using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmClassLibrary
{
    public class Project
    {
        // Database fields
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }

        // Extra fields
        public string Username { get; set; }
        public List<string> Tags { get; set; }


        public Project()
        {
            // Initialize lists
            Tags = new List<string>();
        }
    }
}
