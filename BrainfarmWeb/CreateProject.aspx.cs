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
    public partial class CreatePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCreateNewProject_Click(object sender, EventArgs e)
        {

            // -- reset some fields
            lblError.Text = "";
            lblError.Visible = false;

            // -- get service session token information
            string serviceSessionToken = null;

            if (Session["ServiceSessionToken"] == null) {
                lblError.Text = "You must be logged in to create a new project. <br />";
                lblError.Visible = true;
                return;    
            }
            else {
                serviceSessionToken = (string) Session["ServiceSessionToken"];
            }



            // -- parse tags [FIXME: put in helper method]
            string projectTagsEnteredString = txtProjectTags.Text.Trim();
            projectTagsEnteredString = Regex.Replace(projectTagsEnteredString, @"\s+", " ");
            string[] tagsEntered = projectTagsEnteredString.Split(' ');

            // -- validate empty fields
            bool titleFieldIsEmpty = txtProjectTitle.Text.Trim().Equals("");
            bool tagsFieldIsEmpty = projectTagsEnteredString.Trim().Equals("");
            bool descriptionFieldIsEmpty = txtCreateProjectDescription.Text.Trim().Equals("");

            string errorMessage = "";
            if (titleFieldIsEmpty)
            {
                errorMessage += "You must enter a title for this project! <br />";
            }
            if (tagsFieldIsEmpty)
            {
                errorMessage += "You must enter at least one tag for this project! <br />";
            }
            if (descriptionFieldIsEmpty)
            {
                errorMessage += "You must enter a description for this project! <br />";
            }

            if (!errorMessage.Equals(""))
            {
                lblError.Visible = true;
                lblError.Text = errorMessage;
                return;
            }

            // --- MAKE CALL TO WEB SERVICE
            BrainfarmServiceReference.Project newlyCreatedProject;

            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                try
                {
                    newlyCreatedProject = svc.CreateProject(serviceSessionToken, txtProjectTitle.Text.Trim(),
                                          tagsEntered, txtCreateProjectDescription.Text);
                    
                }
                catch (FaultException ex)
                {
                    lblError.Visible = true;
                    lblError.Text = ex.Message;
                    return;
                }
                catch
                {
                    Response.Redirect("/error/500.html");
                    Response.StatusCode = 500;
                    return;
                }

            }

            // (this is not in the try block, because it will throw some thread aborted exception,
            // which is problematic if it is in a try block)
            Response.Redirect("Project.aspx?ID=" + newlyCreatedProject.ProjectID);

        }
    }
}