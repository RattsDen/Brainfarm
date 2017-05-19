using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using BrainfarmClassLibrary;

namespace BrainfarmService
{
    /*
     * This class defines all of the methods of this service that 
     * can be accessed by another application/process.
     * These methods have been declared in the interface that this
     * class implements.
     * 
     * This class is kind of like the .c file of a C library
     */

    public class BrainfarmService : IBrainfarmService
    {
        
        // This method is pretty much just for connectivity testing
        public string GetTimestamp()
        {
            return DateTime.Now.ToString();
        }

        // Another testing method
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            string sql = @"SELECT * FROM [User]";
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User u = UserDBAccess.ReadUser(reader);
                            users.Add(u);
                        }
                    }
                }
                conn.Close();
            }
            return users;
        }

        public bool RegisterUser(string username, string password, string email)
        {
            // If not valid (empty) username, throw exception
            if (!CheckUsernameRequirements(username))
                throw new FaultException("Username does not meet requirements", new FaultCode("BAD_USERNAME"));

            // If password does not meet requirements, throw exception
            if (!CheckPasswordRequirements(password))
                throw new FaultException("Password does not meet requirements", new FaultCode("BAD_PASSWORD"));

            // If not valid email address, throw exception
            if (!CheckEmailRequirements(email))
                throw new FaultException("Invalid email address", new FaultCode("BAD_EMAIL"));

            // If username already exists, throw exception
            if (UserDBAccess.UsernameExists(username))
                throw new FaultException("Username already in use", new FaultCode("USERNAME_UNAVAILABLE"));

            // If email already exists, throw exception
            if (UserDBAccess.EmailExists(email))
                throw new FaultException("Email address already in use", new FaultCode("EMAIL_UNAVAILABLE"));

            string passwordHash = HashPassword(password);

            bool result = UserDBAccess.InsertUser(username, passwordHash, email);
            return result;
        }

        private bool CheckUsernameRequirements(string username)
        {
            return !string.IsNullOrEmpty(username);
        }

        private bool CheckEmailRequirements(string email)
        {
            // Regex is easy to read ;)
            // address@domain.tld
            string pattern = @"^.+?@.+?\..+?$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }

        private bool CheckPasswordRequirements(string password)
        {
            // This check could stand to be better
            return password.Length >= 8;
        }

        // Hash the input string using SHA512 hashing algorithm
        private string HashPassword(string password)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(password);
            byte[] hash;
            using (var algorithm = new System.Security.Cryptography.SHA512Managed())
            {
                hash = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hash);
        }

        public string Login(string username, string password, bool keepLoggedIn)
        {
            string passwordHash = HashPassword(password);

            // Validate supplied credentials
            User user = UserDBAccess.ValidateCredentials(username, passwordHash);
            if (user == null) // If credentials did not match a user
            {
                throw new FaultException("Incorrect username or password", new FaultCode("BAD_CREDENTIALS"));
            }

            // Create the user session, idenitfied by a session token
            string sessionToken = UserSessionManager.CreateUserSession(user, keepLoggedIn);
            // Return the session token to the consumer
            return sessionToken;
        }

        public User GetCurrentUser(string sessionToken)
        {
            UserSession session = UserSessionManager.GetUserSession(sessionToken);
            if (session != null)
            {
                return session.User; // Session valid - return user
            }
            else
            {
                return null; // Session invalid - return null
            }
        }

        public void Logout(string sessionToken)
        {
            UserSessionManager.ClearSession(sessionToken);
        }
    }
}
