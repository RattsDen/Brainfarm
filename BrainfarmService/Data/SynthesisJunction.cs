using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmService.Data
{
    public class SynthesisJunction
    {
        // Database fields
        public int SynthesisCommentID { get; set; }
        public int LinkedCommentID { get; set; }
        public string SummaryBlurb { get; set; }
    }
}
