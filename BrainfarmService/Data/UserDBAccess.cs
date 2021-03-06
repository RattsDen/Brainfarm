﻿using BrainfarmService.Exceptions;
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

        public User GetUser(int userID)
        {
            string sql = @"
SELECT UserID
      ,Username
      ,CreationDate
      ,Email
  FROM [User]
 WHERE UserID = @UserID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return ReadUser(reader);
                    else
                        throw new EntityNotFoundException(typeof(User));
                }
            }
        }

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
 WHERE Email = @Email AND Email != ''
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Email", email);
                // The email exists if count is greater than 0
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
            }
        }

        public int InsertUser(string username, string passwordHash, string email)
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
      ,@Email);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@Email", email);

                return Convert.ToInt32(command.ExecuteScalar());
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
                        throw new UserAuthenticationException();
                }
            }
        }

        public int UpdateUserEmail(int userID, string newEmail)
        {
            string sql = @"
UPDATE [User]
SET Email = @Email
WHERE UserID = @UserID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Email", newEmail);
                command.Parameters.AddWithValue("@UserID", userID);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int UpdateUserPassword(int userID, string newPassword)
        {
            string sql = @"
UPDATE [User]
SET PasswordHash = @Password
WHERE UserID = @UserID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Password", newPassword);
                command.Parameters.AddWithValue("@UserID", userID);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}