using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BrainfarmWeb.BrainfarmServiceReference;
using ProjectEntity = BrainfarmWeb.BrainfarmServiceReference.Project;
using System.ServiceModel;

namespace BrainfarmWeb
{
    public partial class Account : BrainfarmPage
    {
        protected string sessionToken;
        protected int userID;
        private User currentUser;

        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
            
            currentUser = GetCurrentUser();
            // Set instance variables to be injected into javascript
            sessionToken = GetServiceSessionToken();
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (currentUser == null)
            {
                Response.StatusCode = 400;
                Response.Redirect("/error/400.html");
            }

            try
            {
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    ProjectEntity[] userProjects = svc.GetUserProjects(currentUser.UserID);
                    foreach (Panel panel in LayoutProjectList(userProjects))
                    {
                        panelProjects.Controls.Add(panel);
                    }
                }
            }
            catch
            {

            }

            lblUserName.Text = currentUser.Username;
            txtSetEmail.Text = currentUser.Email;

            lblMessage.Visible = (lblMessage.Text != "");
            lblError.Visible = (lblError.Text != "");

            // Set instance variables to be injected into javascript
            sessionToken = GetServiceSessionToken();
            userID = currentUser.UserID;
        }

        private List<Panel> LayoutProjectList(ProjectEntity[] projects)
        {
            List<Panel> projectPanels = new List<Panel>();
            foreach (ProjectEntity project in projects)
            {
                Panel projectPanel = new Panel();
                projectPanel.CssClass = "div-project";
                projectPanel.Attributes["data-project-id"] = project.ProjectID.ToString();

                // Title label
                Label titleLabel = new Label();
                titleLabel.Text = project.Title;
                titleLabel.CssClass = "project-title";
                projectPanel.Controls.Add(titleLabel);

                Panel infoPanel = new Panel();
                Label usernameLabel = new Label();
                usernameLabel.Text = "by " + project.Username;
                infoPanel.Controls.Add(usernameLabel);
                Label dateLabel = new Label();
                dateLabel.Text = " at " + project.CreationDate.ToString("yyyy-MM-dd h:mm tt");
                infoPanel.Controls.Add(dateLabel);
                projectPanel.Controls.Add(infoPanel);

                // Tags div
                Panel tagsPanel = new Panel();
                foreach (string tag in project.Tags)
                {
                    Label tagLabel = new Label();
                    tagLabel.Text = tag;
                    tagLabel.CssClass = "chip chip-white";
                    tagsPanel.Controls.Add(tagLabel);
                }
                projectPanel.Controls.Add(tagsPanel);

                projectPanels.Add(projectPanel);
            }
            return projectPanels;
        }

        protected void btnUpdateEmail_Click(object sender, EventArgs e)
        {
            if (currentUser != null)
            {
                try
                {
                    using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                    {
                        currentUser = svc.UpdateUserEmail(sessionToken, txtSetEmail.Text);
                        lblMessage.Text = "Email changed successfully";
                        lblError.Text = "";
                    }
                }
                catch (FaultException ex)
                {
                    lblError.Text = ex.Message;
                    lblMessage.Text = "";
                }
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            try
            {
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    currentUser = svc.ChangePassword(sessionToken, txtOldPasswordAuth.Text, txtNewPassword.Text);
                    lblMessage.Text = "Password changed successfully";
                    lblError.Text = "";
                }
            }
            catch (FaultException ex)
            {
                lblError.Text = ex.Message;
                lblMessage.Text = "";
            }
        }
    }
}