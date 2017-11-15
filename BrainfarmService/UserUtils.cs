using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BrainfarmService
{
    /*
     * This class contains helper methods for validating username / password 
     * requirements, hashing passwords, etc.
     */
    public static class UserUtils
    {

        public static bool CheckUsernameRequirements(string username)
        {
            return !string.IsNullOrEmpty(username);
        }

        public static bool CheckEmailRequirements(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return true;
            }
            // Regex is easy to read isn't it?
            // address@domain.tld
            string pattern = @"^[a-zA-Z0-9_.-]+?@[a-zA-Z0-9-_.]+?\..{2,}?$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }

        public static bool CheckPasswordRequirements(string password)
        {
            // This check could stand to be better
            return password.Length >= 8;
        }

        // Hash the input string using SHA512 hashing algorithm
        public static string HashPassword(string password)
        {
            // TODO: Use bcrypt instead of SHA512
            byte[] bytes = new UTF8Encoding().GetBytes(password);
            byte[] hash;
            using (var algorithm = new System.Security.Cryptography.SHA512Managed())
            {
                hash = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hash);
        }

    }
}