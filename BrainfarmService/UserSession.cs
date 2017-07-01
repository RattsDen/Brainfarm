using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrainfarmService.Data;

namespace BrainfarmService
{
    public class UserSession
    {
        public User User { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime RefreshTime { get; set; }
        public bool IsPersistent { get; set; }

        private int sessionTimeToLive; // Minutes (since last refresh)

        public UserSession(User user, bool isPersistent)
        {
            User = user;
            StartTime = DateTime.Now;
            RefreshTime = StartTime;
            IsPersistent = isPersistent;

            sessionTimeToLive = 60; // 60 minute time to live
        }

        /// <summary>
        /// Check if the user session has expired
        /// </summary>
        /// <returns>True if the user session has not yet expired, false if it has.</returns>
        public bool ValidateUserSession()
        {
            // Session is still valid if it is persistent ("remember me" option)
            //or if it has been less that sessionTimeToLive minutes since last refresh
            return IsPersistent || RefreshTime.AddMinutes(sessionTimeToLive) > DateTime.Now;
        }

        /// <summary>
        /// Update the last refreshed time to the current system time.
        /// </summary>
        public void Refresh()
        {
            RefreshTime = DateTime.Now;
        }
    }
}