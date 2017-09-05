using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService
{
    /* Files uploaded to a WCF service can be done so by streaming them 
     * to the service. To do this however, the normal communication structure
     * of SOAP, XML, or JSON must be left out. (You can't just put a .mp3 file into
     * a JSON or XML string. Well... not nicely.)
     * 
     * When a comment is created with a file to be uploaded, this class can be given 
     * a comment ID and a filename and will return a token. You can then stream in 
     * the token, followed by a '\n', followed by the file's data, to map the 
     * uploaded file to the correct comment with the correct filename.
     */
    public static class FileUploadManager
    {
        private static Dictionary<Guid, PendingFileUpload> pendingUploads
            = new Dictionary<Guid, PendingFileUpload>();

        public static String CreatePendingFileUpload(int commentID, string filename)
        {
            PendingFileUpload pending = new PendingFileUpload(commentID, filename);
            Guid guid = Guid.NewGuid();
            pendingUploads.Add(guid, pending);
            return guid.ToString();
        }

        public static PendingFileUpload GetPendingUpload(String token)
        {
            Guid guid = Guid.Parse(token);
            return pendingUploads[guid];
        }

        public static void ClearPendingUpload(String token)
        {
            Guid guid = Guid.Parse(token);
            pendingUploads.Remove(guid);
        }
    }

    public class PendingFileUpload
    {
        // TODO: may need to add a date created variable to handle 
        //       cleaning up abandoned uploads
        public int CommentID { get; set; }
        public string Filename { get; set; }

        public PendingFileUpload(int commentID, string filename)
        {
            this.CommentID = commentID;
            this.Filename = filename;
        }
    }
}