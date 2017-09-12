using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BrainfarmWeb.BrainfarmServiceReference;

namespace BrainfarmWeb
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                try
                {
                    svc.RegisterUser(txtUsername.Text, txtNewPassword.Text, txtEmail.Text);
                    Response.Redirect("Default.aspx");
                }
                catch (Exception ex)
                {
                    lblError.Visible = true;
                    lblError.Text = ex.Message;
                }
            }

            // Don't go sending the password back again
            ClearPasswordFields();
        }

        private void ClearPasswordFields()
        {
            txtNewPassword.Text = "";
            txtPasswordConfirm.Text = "";
        }
    }
}