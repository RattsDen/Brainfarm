using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    /*
     * DTO class for attaching an already uploaded file to a new comment
     */
    public class FileAttachmentRequest
    {
        public int ContributionFileID { get; set; }
        public string Filename { get; set; }
    }
}