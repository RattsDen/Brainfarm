using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmClassLibrary;

namespace BrainfarmService
{
    // Static class to consolidate database access functions for Users
    public static class UserDBAccess
    {
        public static User ReadUser(SqlDataReader reader)
        {
            User user = new User();
            user.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            user.Username = reader.GetString(reader.GetOrdinal("Username"));
            user.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            user.Email = reader.GetString(reader.GetOrdinal("Email"));
            return user;
        }

        public static bool UsernameExists(string username)
        {
            bool result;
            string sql = @"
SELECT COUNT(*)
  FROM [User]
 WHERE Username = @Username
";
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    // The username exists if count is greater than 0
                    result = Convert.ToInt32(command.ExecuteScalar()) != 0;
                }
                conn.Close();
            }

            return result;
        }

        public static bool EmailExists(string email)
        {
            bool result;
            string sql = @"
SELECT COUNT(*)
  FROM [User]
 WHERE Email = @Email
";
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    // The email exists if count is greater than 0
                    result = Convert.ToInt32(command.ExecuteScalar()) != 0;
                }
                conn.Close();
            }

            return result;
        }

        public static bool InsertUser(string username, string passwordHash, string email)
        {
            int result;
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
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Email", email);

                    result = command.ExecuteNonQuery(); // Result = number of rows affected
                }
                conn.Close();
            }

            return result > 0;
        }

        public static User ValidateCredentials(string username, string passwordHash)
        {
            User resultUser;
            string sql = @"
SELECT UserID
      ,Username
      ,CreationDate
      ,Email
  FROM [User]
 WHERE Username = @Username
   AND PasswordHash = @PasswordHash
";
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Only read one result (there should only be one at max)
                            resultUser = ReadUser(reader); // Credentials matched - return user
                        else
                            resultUser = null; // No match - return null
                    }
                }
                conn.Close();
            }

            return resultUser;
        }
    }
}