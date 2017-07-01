using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmService.Data
{
    public class Bookmark
    {
        // Database fields
        public int UserID { get; set; }
        public int CommentID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
