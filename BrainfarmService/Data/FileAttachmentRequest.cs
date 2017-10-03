using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    public class FileAttachmentRequest
    {
        public int ContributionFileID { get; set; }
        public string Filename { get; set; }
    }
}