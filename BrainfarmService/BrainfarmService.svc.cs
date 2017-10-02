using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using BrainfarmService.Data;
using System.IO;
using BrainfarmService.Exceptions;

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

        public User RegisterUser(string username, string password, string email)
        {
            // If not valid (empty) username, throw exception
            if (!UserUtils.CheckUsernameRequirements(username))
                throw new FaultException("Username does not meet requirements", new FaultCode("BAD_USERNAME"));

            // If password does not meet requirements, throw exception
            if (!UserUtils.CheckPasswordRequirements(password))
                throw new FaultException("Password does not meet requirements", new FaultCode("BAD_PASSWORD"));

            // If not valid email address, throw exception
            if (!UserUtils.CheckEmailRequirements(email))
                throw new FaultException("Invalid email address", new FaultCode("BAD_EMAIL"));

            try
            {
                using (UserDBAccess userDBAccess = new UserDBAccess())
                {
                    // If username already exists, throw exception
                    if (userDBAccess.UsernameExists(username))
                        throw new FaultException("Username already in use", new FaultCode("USERNAME_UNAVAILABLE"));

                    // If email already exists, throw exception
                    if (userDBAccess.EmailExists(email))
                        throw new FaultException("Email address already in use", new FaultCode("EMAIL_UNAVAILABLE"));

                    // Get hash of password
                    string passwordHash = UserUtils.HashPassword(password);

                    // Insert the user into the database
                    int userID = userDBAccess.InsertUser(username, passwordHash, email);
                    // Return the new user
                    return userDBAccess.GetUser(userID);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database", new FaultCode("DATABASE_ERROR"));
            }
        }

        public string Login(string username, string password, bool keepLoggedIn)
        {
            string passwordHash = UserUtils.HashPassword(password);

            // Validate supplied credentials
            User user;
            try
            {
                using (UserDBAccess userDBAccess = new UserDBAccess())
                {
                    user = userDBAccess.AuthenticateUser(username, passwordHash);
                }
            }
            catch (UserAuthenticationException)
            {
                throw new FaultException("Incorrect username or password",
                    new FaultCode("BAD_CREDENTIALS"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }

            // Create the user session, idenitfied by a session token
            string sessionToken = LoginTokenManager.GenerateToken(user.UserID, keepLoggedIn);
            // Return the session token to the consumer
            return sessionToken;
        }

        public string RenewToken(string sessionToken)
        {
            return LoginTokenManager.RenewToken(sessionToken);
        }

        public User GetCurrentUser(string sessionToken)
        {
            // Get user ID out of token
            int userId;
            try
            {
                userId = LoginTokenManager.ValidateToken(sessionToken);
            }
            catch (TokenExpiredException)
            {
                throw new FaultException("Your session has expired", new FaultCode("SESSION_EXPIRED"));
            }
            catch (MalformedTokenException)
            {
                throw new FaultException("Invalid session token", new FaultCode("INVALID_SESSION"));
            }

            // Find the user in the database
            try
            {
                using (UserDBAccess userDBAccess = new UserDBAccess())
                {
                    User user = userDBAccess.GetUser(userId);
                    return user;
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("User could not be found", new FaultCode("UNKNOWN_USER"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Project CreateProject(string sessionToken, string title, string[] tags, 
            string firstCommentBody)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (ProjectDBAccess projectDBAccess = new ProjectDBAccess())
                {
                    // Insert project, its tags, and its first comment
                    // Implemented in the ProjectDBAccess class so it doesn't clutter this class
                    //and to keep DB stuff out of this class
                    int projectID = projectDBAccess.CreateProject(user.UserID, title, tags, firstCommentBody);
                    return projectDBAccess.GetProject(projectID);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database", 
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Project GetProject(int projectID)
        {
            try
            {
                using (ProjectDBAccess projectDBAccess = new ProjectDBAccess())
                {
                    return projectDBAccess.GetProject(projectID);
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("Project could not be found", 
                    new FaultCode("UNKNOWN_PROJECT"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Comment CreateComment(string sessionToken, int projectID, int parentCommentID, 
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, string[] fileUploads)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    int commentId = commentDBAccess.CreateComment(projectID, user.UserID, parentCommentID,
                        bodyText, isSynthesis, isContribution, isSpecification,
                        syntheses, fileUploads);
                    return commentDBAccess.GetComment(commentId);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Comment EditComment(string sessionToken, int commentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    int rowsAffected = commentDBAccess.EditComment(commentID, user.UserID, 
                        bodyText, isSynthesis, isContribution, isSpecification);
                    if (rowsAffected == 0)
                    {
                        throw new FaultException("Unable to edit comment #" + commentID + " by user "+user.Username,
                            new FaultCode("COMMENT_NOT_FOUND"));
                    }
                    return commentDBAccess.GetComment(commentID);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public void UploadFile(Stream stream)
        {
            // TODO: implement file uploads
            throw new NotImplementedException();
        }

        public Stream DownloadFile(int contributionFileID)
        {
            // TODO: implement file downloads
            throw new NotImplementedException();
        }

        public List<Comment> GetComments(int projectID, int? parentCommentID)
        {
            // TODO: Flesh all of this out much further
            
            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    return (commentDBAccess.GetComments(projectID, parentCommentID));
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public int RemoveComment(string sessionToken, int commentID)
        {
            User user = GetCurrentUser(sessionToken);
            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    int rowsAffected = commentDBAccess.RemoveComment(commentID, user.UserID);

                    if (rowsAffected == 0)
                    {
                        throw new FaultException("Unable to remove comment",
                            new FaultCode("COMMENT_REMOVE_ERROR"));
                    }

                    return rowsAffected;
                }
            }
            catch (SqlException e)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
            

        }

    }
}
