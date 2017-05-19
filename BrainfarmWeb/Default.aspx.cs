using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BrainfarmClassLibrary;
using BrainfarmWeb.BrainfarmServiceReference;

namespace BrainfarmWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            // Testing connectivity
            /*
            using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
            {
                string timestamp = svc.GetTimestamp();
                lblTime.Text = timestamp;
                User[] users = svc.GetAllUsers();
            }
            */
        }
    }
}