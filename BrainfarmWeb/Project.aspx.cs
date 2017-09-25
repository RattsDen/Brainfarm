using BrainfarmWeb.BrainfarmServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BrainfarmWeb
{
    public partial class Project : System.Web.UI.Page
    {
        protected int projectID;
        protected string sessionToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            sessionToken = (string)Session["ServiceSessionToken"];
            // Get project ID from the request
            if (!int.TryParse(Request.Params["ID"], out projectID))
            {
                // TODO: Should be 400 instead
                Response.StatusCode = 400;
                Response.Redirect("/error/400.html");
            }

            // Get project data from service
            BrainfarmServiceReference.Project project;
            try
            {
                project = GetProject(projectID);
            }
            catch (FaultException ex)
            {
                switch (ex.Code.Name)
                {
                    case "UNKNOWN_PROJECT":
                        {
                            Response.StatusCode = 404;
                            Response.Redirect("/error/404.html");
                            break;
                        }
                    case "DATABASE_ERROR":
                        {
                            Response.StatusCode = 500;
                            Response.Redirect("/error/500.html");
                            break;
                        }
                }
                return;
            }
            catch
            {
                // Something other than a FaultException happened while communicating with the service
                Response.StatusCode = 500;
                Response.Redirect("/error/500.html");
                return;
            }

            // Display project data
            lblProjectTitle.Text = project.Title;
            LayoutProjectTags(project.Tags);
        }
        
        private BrainfarmServiceReference.Project GetProject(int projectID)
        {
            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                return svc.GetProject(projectID);
            }
        }

        private void LayoutProjectTags(string[] tags)
        {
            foreach(string tag in tags)
            {
                Label tagLabel = new Label();
                tagLabel.Text = tag;
                tagLabel.CssClass = "chip";
                panelProjectTags.Controls.Add(tagLabel);
            }
        }

        protected void btnSubmitComment_Click(object sender, EventArgs e)
        {
            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                int parentId = int.Parse(parentCommentId.Value);
                svc.CreateComment((string)Session["ServiceSessionToken"], projectID, parentId, txtComment.Text, chkIsSynthesis.Checked, chkIsContribution.Checked, chkIsSpecification.Checked, null, null);
            }

            clearAllFields();
        }

        private void clearAllFields()
        {
            txtComment.Text = "";
            chkIsSpecification.Checked = false;
            chkIsContribution.Checked = false;
            chkIsSynthesis.Checked = false;
        }
    }
}