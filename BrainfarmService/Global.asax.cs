using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.SessionState;

namespace BrainfarmService
{
    // This is where you override certain events such as the service starting up or shutting down.

    public class Global : System.Web.HttpApplication
    {
        private const string CACHE_KEY_SESSION_CLEANUP = "UserSessionCleanup";

        protected void Application_Start(object sender, EventArgs e)
        {
            // Begin scheduling user session cleanups when the service starts
            ScheduleUserSessionCleanup();
        }

        private void ScheduleUserSessionCleanup()
        {
            DateTime scheduledTime = DateTime.Now.AddMinutes(60); // Every 60 minutes

            HttpRuntime.Cache.Insert(CACHE_KEY_SESSION_CLEANUP, scheduledTime, null, scheduledTime, 
                Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, 
                new CacheItemRemovedCallback(UserSessionCleanupDelegate));
        }

        public void UserSessionCleanupDelegate(string key, object value, CacheItemRemovedReason reason)
        {
            // Clean up expired user sessions
            UserSessionManager.ClearExpiredSessions();
            // Reschedule cleanup
            ScheduleUserSessionCleanup();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // When an http POST, PUT, or DELETE request is made to another domain via AJAX
            //(ie. javascript on MyWebsite.com calls out to Google.com) a "preflight" http
            //OPTIONS request is sent first. 
            // This code allows the service to not choke on http OPTIONS requests when an 
            //AJAX call is made to it.
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT, DELETE");

                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }
    }
}