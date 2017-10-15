using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    /*
     * DTO class for creating a synthesis junction between an 
     * existing comment and a new comment
     */
    public class SynthesisRequest
    {
        public int LinkedCommentID { get; set; }
        public string Subject { get; set; }
    }
}