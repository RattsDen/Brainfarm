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
    public partial class Default : BrainfarmPage
    {
        protected string sessionToken;

        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            sessionToken = GetServiceSessionToken();
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
                            = svc.GetRecommendedProjects(sessionToken, 5);
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
    }
}