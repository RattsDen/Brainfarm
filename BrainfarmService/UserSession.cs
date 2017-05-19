using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrainfarmClassLibrary;

namespace BrainfarmService
{
    public class UserSession
    {
        public User User { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime RefreshTime { get; set; }
        public bool IsPersistent { get; set; }

        public UserSession(User user, bool isPersistent)
        {
            User = user;
            StartTime = DateTime.Now;
            RefreshTime = StartTime;
            IsPersistent = isPersistent;
        }

        public bool ValidateUserSession()
        {
            return true;
        }

        public void Refresh()
        {
            RefreshTime = DateTime.Now;
        }
    }
}