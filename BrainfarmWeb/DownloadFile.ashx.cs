using BrainfarmWeb.BrainfarmServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace BrainfarmWeb
{
    /*
     * Resource for downloading a specified Contribution File from the
     * Brainfarm web service in a browser-compatible way
     */
    public class DownloadFile : IHttpHandler
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            // Get parameters from request
            int contributionFileID;
            string filename;
            try
            {
                contributionFileID = int.Parse(context.Request.Params["ID"]);
                filename = context.Request.Params["Filename"];
            }
            catch
            {
                context.Response.StatusCode = 400;
                context.Response.Redirect("/error/400.html");
                return;
            }

            // Get file stream from service
            Stream stream;
            try
            {
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    stream = svc.DownloadFile(contributionFileID);
                }
            }
            catch (FaultException ex)
            {
                switch (ex.Code.Name)
                {
                    case "UNKNOWN_CONTRIBUTION_FILE":
                        {
                            context.Response.StatusCode = 404;
                            context.Response.Redirect("/error/404.html");
                            break;
                        }
                    case "DATABASE_ERROR":
                        {
                            context.Response.StatusCode = 500;
                            context.Response.Redirect("/error/500.html");
                            break;
                        }
                }
                return;
            }
            catch
            {
                // Something other than a FaultException happened while communicating with the service
                context.Response.StatusCode = 500;
                context.Response.Redirect("/error/500.html");
                return;
            }

            // Write the file to the response with appropriate headers
            try
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
                context.Response.ContentType = "application/octet-stream";
                stream.CopyTo(context.Response.OutputStream);
            }
            catch
            {
                context.Response.StatusCode = 500;
                context.Response.Redirect("/error/500.html");
                return;
            }
            context.Response.End();
        }   
    }
}