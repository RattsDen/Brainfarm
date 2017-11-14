using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    public class Rating
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int Weight { get; set; }
        public DateTime CreationDate { get; set; }
    }
}