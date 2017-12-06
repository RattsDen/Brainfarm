using BrainfarmWeb.BrainfarmServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BrainfarmWeb
{
    public partial class Project : BrainfarmPage
    {
        protected int projectID;
        protected BrainfarmServiceReference.Project project;
        protected string sessionToken;

        protected override void Page_Load(object sender, EventArgs e)
        {
            
            base.Page_Load(sender, e);

            // Get project ID from the request
            if (!int.TryParse(Request.Params["ID"], out projectID))
            {
                Response.StatusCode = 400;
                Response.Redirect("/error/400.html");
            }

            // Moved to PageLoad_Complete
            //LoadPageData();
        }

        private void LoadPageData()
        {
            // Get project data from service
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
            txtProjectTitle.Text = project.Title;
            LayoutProjectTags(project.Tags);
            txtProjectTags.Text = string.Join(" ", project.Tags);
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

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            LoadPageData();

            sessionToken = GetServiceSessionToken();
            // Show edit button if current user is project owner
            BrainfarmServiceReference.User user = GetCurrentUser();
            if (user != null && user.UserID == project.UserID)
            {
                panelEditButton.Visible = true;
            }
        }

        protected void btnEditProjectSubmit_Click(object sender, EventArgs e)
        {
            lblEditProjectError.Visible = false;
            if (string.IsNullOrEmpty(txtProjectTitle.Text.Trim()))
            {
                lblEditProjectError.Visible = true;
                lblEditProjectError.Text = "Title must not be empty";
                return;
            }
            if (string.IsNullOrEmpty(txtProjectTags.Text.Trim()))
            {
                lblEditProjectError.Visible = true;
                lblEditProjectError.Text = "You must enter at least one tag";
                return;
            }

            string title = txtProjectTitle.Text;
            string[] tags = Regex.Replace(txtProjectTags.Text.Trim(), @"\s+", " ").Split(' ');

            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                try
                {
                    svc.EditProject(GetServiceSessionToken(), projectID, title, tags);
                }
                catch (FaultException ex)
                {
                    lblEditProjectError.Visible = true;
                    lblEditProjectError.Text = ex.Message;
                }
                catch
                {
                    // Something other than a FaultException happened while communicating with the service
                    Response.StatusCode = 500;
                    Response.Redirect("/error/500.html");
                    return;
                }
            }
        }
    }
}