using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmClassLibrary
{
    public class SynthesisJunction
    {
        // Database fields
        public int SynthesisCommentID { get; set; }
        public int LinkedCommentID { get; set; }
        public string SummaryBlurb { get; set; }
    }
}
