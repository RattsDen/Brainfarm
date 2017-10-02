using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmService.Data
{
    public class Comment
    {
        // Database fields
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int? ParentCommentID { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? EditedDate { get; set; }
        public string BodyText { get; set; }
        public bool IsSynthesis { get; set; }
        public bool IsContribution { get; set; }
        public bool IsSpecification { get; set; }
        public bool IsRemoved { get; set; }

        // Extra fields
        public string Username { get; set; }
        public bool IsBookmarked { get; set; }
        public List<Comment> Children { get; set; }
        public List<int> LinkingCommentIDs { get; set; } // Synthesis comments that link to this
        public List<SynthesisJunction> Syntheses { get; set; } // Comments that this synthesis links to
        public List<ContributionFile> ContributionFiles { get; set; }


        public Comment()
        {
            // Initialize lists
            Children = new List<Comment>();
            LinkingCommentIDs = new List<int>();
            Syntheses = new List<SynthesisJunction>();
            ContributionFiles = new List<ContributionFile>();
        }
    }
}
