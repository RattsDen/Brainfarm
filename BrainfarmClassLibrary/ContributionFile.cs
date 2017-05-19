using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmClassLibrary
{
    public class ContributionFile
    {
        // Database fields
        public int ContributionFileID { get; set; }
        public int CommentID { get; set; }
        public string Filename { get; set; }
    }
}
