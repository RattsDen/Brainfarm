using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BrainfarmWeb.BrainfarmServiceReference;
using System.ServiceModel;

namespace BrainfarmWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
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