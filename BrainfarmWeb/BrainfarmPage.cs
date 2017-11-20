using BrainfarmWeb.BrainfarmServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace BrainfarmWeb
{
    public class BrainfarmPage : System.Web.UI.Page
    {
        protected virtual void Page_Load(object sender, EventArgs e)
        {
            // Attempt to refresh the current session token if one exists
            string oldToken = GetServiceSessionToken();
            if (oldToken != null)
            {
                try
                {
                    using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                    {
                        string newToken = svc.RenewToken(oldToken);
                        if (newToken != oldToken)
                        {
                            // Token changed
                            SetServiceSessionToken(newToken);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public string GetServiceSessionToken()
        {
            //return (string)Session["ServiceSessionToken"];
            HttpCookie cookie = Request.Cookies.Get("ServiceSessionToken");
            if (cookie != null)
                return cookie.Value;
            else
                return null;
        }

        public void SetServiceSessionToken(string sessionToken)
        {
            //Session["ServiceSessionToken"] = sessionToken;
            HttpCookie cookie = new HttpCookie("ServiceSessionToken", sessionToken);
            if (sessionToken == null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1); // set to expire
            }
            Response.Cookies.Add(cookie);
            
        }

        public User GetCurrentUser()
        {
            User currentUser = null;
            if (GetServiceSessionToken() != null)
            {
                // Service session token exists - user should be logged in
                using (BrainfarmServiceClient svc = new BrainfarmServiceClient())
                {
                    try
                    {
                        currentUser = svc.GetCurrentUser(GetServiceSessionToken());
                    }
                    catch (FaultException)
                    {
                        currentUser = null;
                    }
                    catch
                    {
                        Response.StatusCode = 500;
                        Response.Redirect("/error/500.html");
                        return null;
                    }
                }
                // If service responds with null, clear the attempted session token and return to the homepage
                // Essentially, if the session is no longer valid, make it look like a logout
                if (currentUser == null)
                {
                    SetServiceSessionToken(null);
                    Response.Redirect("/Default.aspx");
                    return null;
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