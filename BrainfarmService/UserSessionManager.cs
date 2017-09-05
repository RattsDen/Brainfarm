using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using BrainfarmService.Data;

namespace BrainfarmService
{
    /*
     * This class stores user session information in memory, providing
     * tokens to access the session information.
     * 
     * Because it is stored in memory, it is not particularly scalable. 
     * (Other instances of the service won't be able to share sessions 
     * with eachother.) This can be relieved somewhat by storing it in 
     * a database, using a centralized session storing server, or by 
     * storing session information client side instead
     */
    public static class UserSessionManager
    {
        // Table storing user sessions
        // GUIDs are used as a key which may or may not be secure according 
        //to back-and-forth discussions on Stack Overflow.
        private static Dictionary<Guid, UserSession> activeSessions 
            = new Dictionary<Guid, UserSession>();

        /// <summary>
        /// Creates and stores a UserSession object for the specified user.
        /// </summary>
        /// <param name="user">User to store login session for</param>
        /// <param name="isPersistent">Whether or not the session should expire</param>
        /// <returns>Session token in the form of a string</returns>
        public static string CreateUserSession(User user, bool isPersistent)
        {
            UserSession session = new UserSession(user, isPersistent);
            Guid guid = Guid.NewGuid();
            activeSessions.Add(guid, session);
            return guid.ToString();
        }

        /// <summary>
        /// Retrieve the user session associated with the specified key.
        /// </summary>
        /// <param name="sessionToken">Key to retrieve session for</param>
        /// <returns>
        /// A UserSession object. 
        /// Returns null if session does not exist, is expired, or if the token is invalid
        /// </returns>
        public static UserSession GetUserSession(string sessionToken)
        {
            Guid guid;
            UserSession session;

            if (!Guid.TryParse(sessionToken, out guid))
                return null; // Invalid token

            if (!activeSessions.TryGetValue(guid, out session))
                return null; // Session does not exist

            if (!session.ValidateUserSession())
            {
                // Session existed but had expired
                // Remove from table and return null
                ClearSession(sessionToken);
                return null;
            }
            
            session.Refresh(); // Update refresh time on session
            return session;
        }

        /// <summary>
        /// Remove the session associated with the specified key from the table.
        /// </summary>
        /// <param name="sessionToken">Key to remove session for</param>
        public static void ClearSession(string sessionToken)
        {
            Guid guid;
            if (Guid.TryParse(sessionToken, out guid))
            {
                activeSessions.Remove(guid);
            }
        }

        /// <summary>
        /// Remove all expired sessions from the table.
        /// </summary>
        public static void ClearExpiredSessions() {
            foreach (KeyValuePair<Guid, UserSession> entry in activeSessions)
            {
                if (!entry.Value.ValidateUserSession())
                {
                    activeSessions.Remove(entry.Key);
                }
            }
        }
    }
}