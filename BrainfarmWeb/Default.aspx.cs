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
    public partial class Default : System.Web.UI.Page
    {
        protected string sessionToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            sessionToken = (string)Session["ServiceSessionToken"];
            User currentUser = GetCurrentUser();

            // Display propper dashboard content
            if (currentUser != null)
            {
                // Logged in
                lblUsername.Text = currentUser.Username;

                panelLoggedOut.Visible = false;
                panelLoggedIn.Visible = true;
                panelRecommended.Visible = true;
                panelBookmarks.Visible = true;
            }
            else
            {
                // Logged out
                panelLoggedOut.Visible = true;
                panelLoggedIn.Visible = false;
                panelRecommended.Visible = false;
                panelBookmarks.Visible = false;
            }

            try
            {
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    // Get popular projects
                    ProjectEntity[] popularProjects 
                        = svc.GetPopularProjects(5);
                    // Display popular projects
                    foreach (Panel panel in LayoutProjectList(popularProjects))
                    {
                        panelPopular.Controls.Add(panel);
                    }

                    if (currentUser != null)
                    {
                        // Get reccomended projects for current user
                        ProjectEntity[] recommendedProjects
                            = svc.GetRecommendedProjects(currentUser.UserID, 5);
                        // Display recommended projects
                        foreach (Panel panel in LayoutProjectList(recommendedProjects))
                        {
                            panelRecommended.Controls.Add(panel);
                        }
                    }
                }
            }
            catch
            {

            }
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

        private User GetCurrentUser()
        {
            User currentUser = null;

            // Get current user
            if (Session["ServiceSessionToken"] != null)
            {
                // Service session token exists - user should be logged in
                try
                {
                    using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                    {
                        currentUser = svc.GetCurrentUser((string)Session["ServiceSessionToken"]);
                    }
                }
                catch (FaultException ex)
                {
                    currentUser = null;
                }
                catch
                {
                    Response.StatusCode = 500;
                    Response.Redirect("/error/500.html");
                }
                // If service responds with null, clear the attempted session token and return to the homepage
                // Essentially, if the session is no longer valid, make it look like a logout
                if (currentUser == null)
                {
                    Session.Remove("ServiceSessionToken");
                    Response.Redirect("/Default.aspx");
                }
            }
            else
            {
                // Service session token does not exist - user not logged in
                currentUser = null;
            }

            return currentUser;
        }
    }
}