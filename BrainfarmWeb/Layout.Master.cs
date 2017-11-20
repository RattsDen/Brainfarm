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

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            SetHeaderContent();
        }

        private void SetHeaderContent()
        {
            User currentUser;
            if (this.Page is BrainfarmPage)
            {
                currentUser = ((BrainfarmPage)this.Page).GetCurrentUser();
            }
            else
            {
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
                    ((BrainfarmPage)this.Page).SetServiceSessionToken(serviceSessionToken);
                }
                catch (FaultException ex)
                {
                    lblLoginError.Text = ex.Message;
                    lblLoginError.Visible = true;
                }
                catch
                {
                    Response.StatusCode = 500;
                    Response.Redirect("/error/500.html");
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
            if (((BrainfarmPage)this.Page) != null)
            {
                // Service uses JWTs to track state. 
                // Logging out using JWTs simply meants throwing away your token.
                ((BrainfarmPage)this.Page).SetServiceSessionToken(null);
            }
        }

    }
}