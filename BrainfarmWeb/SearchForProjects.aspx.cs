﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BrainfarmWeb
{
    public partial class SearchForProjects : BrainfarmPage
    {
        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // reset error message field
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;

            // validate input
            string errorMessage = "";
            if (txtSearchKeywords.Text.Trim().Equals(""))
            {
                errorMessage += "You must enter some search keywords!";
            }

            if (!errorMessage.Equals(""))
            {
                lblErrorMessage.Text = errorMessage;
                lblErrorMessage.Visible = true;
            }

            // check a checkbox if none are checked
            if (!checkboxTags.Checked && !checkboxTitles.Checked)
            {
                checkboxTags.Checked = true;
            }

            // return if an error message was displayed
            if (!errorMessage.Equals(""))
            {
                return;
            }

            // get the list of projects that result from doing search
            BrainfarmServiceReference.Project[] searchResults;

            using (BrainfarmServiceReference.BrainfarmServiceClient svc 
                = new BrainfarmServiceReference.BrainfarmServiceClient())
            {
                searchResults = svc.SearchProjects(txtSearchKeywords.Text.Trim(), checkboxTags.Checked, checkboxTitles.Checked);
            }

            foreach (Panel resultPanel in LayoutProjectList(searchResults))
            {
                resultsPanel.Controls.Add(resultPanel);
            }

            /*
            foreach (BrainfarmServiceReference.Project projectElement in searchResults)
            {
                Panel projectResultPanel = new Panel();
                projectResultPanel.Attributes.Add("projectSearchResultId", projectElement.ProjectID.ToString());
                projectResultPanel.Attributes.Add("class", "div-project");
                projectResultPanel.Attributes.Add("onclick", "showProjectPage(" + projectElement.ProjectID.ToString() + ")");
            
                // -- create title div
                Panel titleDiv = new Panel();

                Label usernameLabel = new Label();
                usernameLabel.Text += projectElement.Username;
                usernameLabel.Attributes.Add("class", "project-username");

                Label creationDateLabel = new Label();
                creationDateLabel.Text += projectElement.CreationDate;
                creationDateLabel.Attributes.Add("class", "project-creationdate");

                Label titleLabel = new Label();
                titleLabel.Text += projectElement.Title;
                titleLabel.Attributes.Add("class", "project-title");

                titleDiv.Controls.Add(usernameLabel);
                titleDiv.Controls.Add(creationDateLabel);
                titleDiv.Controls.Add(titleLabel);

                // -- create tags div
                Panel tagsDiv = new Panel();

                foreach (string tagText in projectElement.Tags)
                {
                    Label tagDiv = new Label();
                    tagDiv.Text = tagText;
                    tagDiv.Attributes.Add("class", "chip-white chip");

                    tagsDiv.Controls.Add(tagDiv);
                }

                // put sub-divs into project search results div
                projectResultPanel.Controls.Add(titleDiv);
                projectResultPanel.Controls.Add(tagsDiv);

                // add this project search result div to webpage
                resultsPanel.Controls.Add(projectResultPanel);

            }
            */

        }

        private List<Panel> LayoutProjectList(BrainfarmServiceReference.Project[] projects)
        {
            List<Panel> projectPanels = new List<Panel>();
            foreach (BrainfarmServiceReference.Project project in projects)
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