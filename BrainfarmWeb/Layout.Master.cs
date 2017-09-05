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
    public partial class Layout : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetHeaderContent();
        }

        private void SetHeaderContent()
        {
            User currentUser;

            // Get current user
            if (Session["ServiceSessionToken"] != null)
            {
                // Service session token exists - user should be logged in
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    try
                    {
                        currentUser = svc.GetCurrentUser((string)Session["ServiceSessionToken"]);
                    }
                    catch (FaultException ex)
                    {
                        currentUser = null;
                    }
                    catch
                    {
                        Response.StatusCode = 500;
                        Response.Redirect("/500.html");
                        return;
                    }
                }
                // If service responds with null, clear the attempted session token and return to the homepage
                // Essentially, if the session is no longer valid, make it look like a logout
                if (currentUser == null)
                {
                    Session.Remove("ServiceSessionToken");
                    Response.Redirect("/Default.aspx");
                    return;
                }
            }
            else
            {
                // Service session token does not exist - user not logged in
                currentUser = null;
            }

            // Display propper header content
            if (currentUser != null)
            {
                panelLogin.Visible = false;
                panelCurrentUser.Visible = true;
                lblCurrentUser.Text = currentUser.Username;
            }
            else
            {
                panelLogin.Visible = true;
                panelCurrentUser.Visible = false;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            Login(txtUsername.Text, txtPassword.Text, chkRemember.Checked);
            SetHeaderContent();
        }

        private void Login(string username, string password, bool remember)
        {
            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                try
                {
                    string serviceSessionToken = svc.Login(username, password, remember);
                    Session["ServiceSessionToken"] = serviceSessionToken;
                }
                catch (FaultException ex)
                {
                    lblLoginError.Text = ex.Message;
                    lblLoginError.Visible = true;
                }
                catch
                {
                    Response.StatusCode = 500;
                    Response.Redirect("/500.html");
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Logout();
            Response.Redirect("/Default.aspx");
        }

        private void Logout()
        {
            if (Session["ServiceSessionToken"] != null)
            {
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    try
                    {
                        svc.Logout((string)Session["ServiceSessionToken"]);
                    }
                    catch (FaultException ex)
                    {
                        lblLoginError.Text = ex.Message;
                        lblLoginError.Visible = true;
                    }
                    catch
                    {
                        Response.StatusCode = 500;
                        Response.Redirect("/500.html");
                    }
                }
                Session.Remove("ServiceSessionToken");
            }
        }

    }
}