using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    // Static class to consolidate database access functions for Users
    public class UserDBAccess : DBAccess
    {
        public UserDBAccess() : base() { }
        public UserDBAccess(DBAccess parent) : base(parent) { }

        public static User ReadUser(SqlDataReader reader)
        {
            User user = new User();
            user.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            user.Username = reader.GetString(reader.GetOrdinal("Username"));
            user.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            user.Email = reader.GetString(reader.GetOrdinal("Email"));
            return user;
        }

        public bool UsernameExists(string username)
        {
            string sql = @"
SELECT COUNT(*)
  FROM [User]
 WHERE Username = @Username
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Username", username);
                // The username exists if count is greater than 0
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
            }
        }

        public bool EmailExists(string email)
        {
            string sql = @"
SELECT COUNT(*)
  FROM [User]
 WHERE Email = @Email
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Email", email);
                // The email exists if count is greater than 0
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
            }
        }

        public bool InsertUser(string username, string passwordHash, string email)
        {
            string sql = @"
INSERT INTO [User]
      (Username
      ,PasswordHash
      ,CreationDate
      ,Email)
VALUES(@Username
      ,@PasswordHash
      ,@CreationDate
      ,@Email)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@Email", email);

                return command.ExecuteNonQuery() > 0; // Result = number of rows affected > 0
            }
        }

        public User AuthenticateUser(string username, string passwordHash)
        {
            string sql = @"
SELECT UserID
      ,Username
      ,CreationDate
      ,Email
  FROM [User]
 WHERE Username = @Username
   AND PasswordHash = @PasswordHash
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Only read one result (there should only be one at max)
                        return ReadUser(reader); // Credentials matched - return user
                    else
                        return null; // TODO: Throw exception here
                }
            }
        }
    }
}