using BrainfarmService.Data;
using JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BrainfarmService
{
    /* 
     * This class handles the generation and validation of JSON Web Tokens (JWTs) 
     * used to validate the user with the service.
     * 
     * JWTs store the session information (UserID) inside themselves and are stored 
     * client side. This allows multiple instances of the service to be created 
     * without needing to share user session data between them, because the client
     * is storing it themselves.
     * 
     * JWTs can be stored on the client side safely because they are given a 
     * cryptographic signature by the server, ensuring that the token was created 
     * by an instance of the service.
     */

    public static class LoginTokenManager
    {
        private static DateTime unixEpoch = new DateTime(1970, 1, 1);
        private static int shortExpireTime = 60; // Minutes
        private static int shortRefreshTime = 90;
        private static int longExpireTime = 1440; // 1 day
        private static int longRefreshTime = 43200; // 30 days

        private static string secret = "RicciAndrewTaylorThompsonApostropheScottDavidEricNijjar";

        public static string GenerateToken(int userID, bool isPersistent)
        {
            int exp, refreshExp;
            if (isPersistent)
            {
                exp = longExpireTime;
                refreshExp = longRefreshTime;
            }
            else
            {
                exp = shortExpireTime;
                refreshExp = shortRefreshTime;
            }

            Dictionary<string, object> claims = new Dictionary<string,object>();
            claims.Add("UserID", userID);
            claims.Add("IsPersistent", isPersistent);
            // Store time as seconds since unix epoch, as is JWT standard (for some reason)
            claims.Add("exp", (int)(DateTime.UtcNow.AddMinutes(exp) - unixEpoch).TotalSeconds);
            claims.Add("refreshExp", (int)(DateTime.UtcNow.AddMinutes(refreshExp) - unixEpoch).TotalSeconds);

            return JsonWebToken.Encode(claims, secret, JwtHashAlgorithm.HS256);
        }

        public static int ValidateToken(string token)
        {
            if(token == "")
            {
                throw new EmptyTokenException();
            }
            try
            {
                Dictionary<string, object> claims
                    = JsonWebToken.DecodeToObject<Dictionary<string, object>>(token, secret, true);

                // Check that token is not expired
                DateTime exp = unixEpoch.AddSeconds((int)claims["exp"]);
                if (exp < DateTime.UtcNow)
                    throw new TokenExpiredException();

                return (int)claims["UserID"];
            }
            catch
            {
                throw new MalformedTokenException();
            }
        }

        // If token is not expired, return the same token
        // If token is expired and valid for refresh, return new token
        // If token is not valid for refresh, throw exception
        public static string RenewToken(string token)
        {
            try
            {
                Dictionary<string, object> claims
                    = JsonWebToken.DecodeToObject<Dictionary<string, object>>(token, secret, true);

                // Check whether or not the token is expired
                DateTime exp = unixEpoch.AddSeconds((int)claims["exp"]);
                if (exp < DateTime.UtcNow)
                {
                    // If the token is expired, see if it can be refreshed
                    DateTime refreshExp = unixEpoch.AddSeconds((int)claims["refreshExp"]);
                    if (refreshExp >= DateTime.UtcNow)
                    {
                        int userID = (int)claims["UserID"];
                        bool isPersistent = (bool)claims["IsPersistent"];
                        return GenerateToken(userID, isPersistent);
                    }
                    else
                    {
                        throw new TokenExpiredException();
                    }
                }
                else
                {
                    // Token is not expired, return original token
                    return token;
                }
            }
            catch
            {
                throw new MalformedTokenException();
            }
        }
    }

    public class TokenExpiredException : Exception { }
    public class MalformedTokenException : Exception { }
    public class EmptyTokenException : Exception { }
}