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

        public User UpdateUserEmail(string sessionToken, string newEmail)
        {
            // If not valid email address, throw exception
            if (!UserUtils.CheckEmailRequirements(newEmail))
                throw new FaultException("Invalid email address", new FaultCode("BAD_EMAIL"));

            User currentUser = GetCurrentUser(sessionToken);
            
            using (UserDBAccess userDBAccess = new UserDBAccess())
            {
                userDBAccess.UpdateUserEmail(currentUser.UserID, newEmail);
            }

            return GetCurrentUser(sessionToken);
        }

        public User ChangePassword(string sessionToken, string oldPassword, string newPassword)
        {
            if (!UserUtils.CheckPasswordRequirements(newPassword))
                throw new FaultException("Password does not meet requirements", new FaultCode("BAD_PASSWORD"));

            string oldPasswordHash = UserUtils.HashPassword(oldPassword);
            string newPasswordHash = UserUtils.HashPassword(newPassword);
            
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (UserDBAccess userDBAccess = new UserDBAccess())
                {
                    userDBAccess.AuthenticateUser(user.Username, oldPasswordHash);
                    userDBAccess.UpdateUserPassword(user.UserID, newPasswordHash);
                    return GetCurrentUser(sessionToken);
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
            catch (EmptyTokenException)
            {
                throw new FaultException("No session token was provided. User is not logged in", new FaultCode("NO_SESSION_TOKEN"));
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
            SynthesisRequest[] syntheses, FileAttachmentRequest[] attachments)
        {
            if (bodyText == null || bodyText == "")
            {
                throw new FaultException("Comment Body must not be empty",
                    new FaultCode("MISSING_COMMENT_BODY"));
            }

            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    int commentId = commentDBAccess.CreateComment(projectID, user.UserID, parentCommentID,
                        bodyText, isSynthesis, isContribution, isSpecification,
                        syntheses, attachments);
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
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, FileAttachmentRequest[] attachments)
        {
            if (bodyText == null || bodyText == "")
            {
                throw new FaultException("Comment Body must not be empty",
                    new FaultCode("MISSING_COMMENT_BODY"));
            }

            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    // Throw exception if user is not comment owner
                    if (commentDBAccess.GetComment(commentID).UserID != user.UserID)
                        throw new FaultException("You do not have permission to do that",
                            new FaultCode("INVALID_PERMISSIONS"));

                    int rowsAffected = commentDBAccess.EditComment(commentID, user.UserID, 
                        bodyText, isSynthesis, isContribution, isSpecification, 
                        syntheses, attachments);
                    if (rowsAffected == 0)
                    {
                        throw new FaultException("Unable to edit comment",
                            new FaultCode("COMMENT_NOT_EDITED"));
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

        public ContributionFile UploadFile(Stream stream)
        {
            try
            {
                using (ContributionFileDBAccess contributionFileDBAccess = new ContributionFileDBAccess())
                {
                    return contributionFileDBAccess.InsertContributionFile(stream);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Stream DownloadFile(int contributionFileID)
        {
            try
            {
                using (ContributionFileDBAccess contributionFileDBAccess = new ContributionFileDBAccess())
                {
                    return contributionFileDBAccess.GetFileContents(contributionFileID);
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("File could not be found",
                    new FaultCode("UNKNOWN_CONTRIBUTION_FILE"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
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

        public List<Project> GetPopularProjects(int top)
        {
            try
            {
                using (ProjectDBAccess projectDBAccess = new ProjectDBAccess())
                {
                    return projectDBAccess.GetPopularProjects(top);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Project> GetRecommendedProjects(int userID, int top)
        {
            try
            {
                using (ProjectDBAccess projectDBAccess = new ProjectDBAccess())
                {
                    return projectDBAccess.GetRecommendedProjects(userID, top);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Project> GetUserProjects(int userID)
        {
            try
            {
                using (DBAccess dbAccess = new DBAccess())
                {
                    UserDBAccess userDBAccess = new UserDBAccess(dbAccess);
                    userDBAccess.GetUser(userID); // Check if user exists
                    ProjectDBAccess projectDBAccess = new ProjectDBAccess(dbAccess);
                    return projectDBAccess.GetUserProjects(userID);
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("User could not be found", 
                    new FaultCode("UNKNOWN_USER"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Comment> GetUserComments(int userID)
        {
            try
            {
                using (DBAccess dbAccess = new DBAccess())
                {
                    UserDBAccess userDBAccess = new UserDBAccess(dbAccess);
                    userDBAccess.GetUser(userID); // Check if user exists
                    CommentDBAccess commentDBAccess = new CommentDBAccess(dbAccess);
                    return commentDBAccess.GetUserComments(userID);
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("User could not be found",
                    new FaultCode("UNKNOWN_USER"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Project> SearchProjects(string searchKeywordsString, bool searchTags, bool searchTitles)
        {
            // -- corner case
            if (!searchTags && !searchTitles)
            {
                return new List<Project>();
            }

            // -- typical (non-corner) case
            using (ProjectDBAccess myProjectDBAccess = new ProjectDBAccess())
            {
                return myProjectDBAccess.GetProjectsWithAllTheseSearchKeywordsInTagsOrTitle(
                    searchKeywordsString, searchTags, searchTitles
                    );
            }
        }

        public int RemoveComment(string sessionToken, int commentID)
        {
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (CommentDBAccess commentDBAccess = new CommentDBAccess())
                {
                    // Throw exception if user is not comment owner
                    if (commentDBAccess.GetComment(commentID).UserID != user.UserID)
                        throw new FaultException("You do not have permission to do that",
                            new FaultCode("INVALID_PERMISSIONS"));

                    int rowsAffected = commentDBAccess.RemoveComment(commentID);

                    if (rowsAffected == 0)
                    {
                        throw new FaultException("Unable to remove comment",
                            new FaultCode("COMMENT_REMOVE_ERROR"));
                    }

                    return rowsAffected;
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Bookmark BookmarkComment(string sessionToken, int commentID)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (DBAccess dbAccess = new DBAccess())
                {
                    BookmarkDBAccess bookmarkDBAccess = new BookmarkDBAccess(dbAccess);
                    CommentDBAccess commentDBAccess = new CommentDBAccess(dbAccess);

                    // Test to see if comment exists - will throw exception if not
                    commentDBAccess.GetComment(commentID);

                    // Test to see if bookmark already exists
                    if (bookmarkDBAccess.BookmarkExists(user.UserID, commentID))
                        throw new FaultException("Bookmark already exists",
                            new FaultCode("DUPLICATE_BOOKMARK"));

                    bookmarkDBAccess.BookmarkComment(user.UserID, commentID);
                    return bookmarkDBAccess.GetBookmark(user.UserID, commentID);
                }
            }
            catch (EntityNotFoundException ex)
            {
                if (ex.EntityType == typeof(Comment))
                {
                    throw new FaultException("Comment does not exist",
                        new FaultCode("UNKNOWN_COMMENT"));
                }
                else // ex.EntityType == typeof(Rating)
                {
                    throw new FaultException("Bookmark could not be added",
                        new FaultCode("BOOKMARK_NOT_CREATED"));
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }


        public Bookmark UnbookmarkComment(string sessionToken, int commentID)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (BookmarkDBAccess bookmarkDBAccess = new BookmarkDBAccess())
                {
                    Bookmark bookmark = bookmarkDBAccess.GetBookmark(user.UserID, commentID);
                    bookmarkDBAccess.UnbookmarkComment(user.UserID, commentID);
                    return bookmark;
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("Bookmark could not be removed because it does not exist",
                    new FaultCode("UNKNOWN_BOOKMARK"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        // returns a list of commentIds that this user bookmarked on this project
        public List<int> GetBookmarksForProject(string sessionToken, int projectID)
        {
            // Get user from session
            User user = GetCurrentUser(sessionToken);

            // TODO: maybe (?) throw exception if project does not exist

            try
            {
                using (BookmarkDBAccess bookmarkDBAccess = new BookmarkDBAccess())
                {
                    return bookmarkDBAccess.GetBookmarksForProject(user.UserID, projectID);
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Comment> GetUserBookmarkedComments(string sessionToken)
        {
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (DBAccess dbAccess = new DBAccess())
                {
                    BookmarkDBAccess bookmarkDBAccess = new BookmarkDBAccess(dbAccess);
                    CommentDBAccess commentDBAccess = new CommentDBAccess(dbAccess);

                    List<Comment> results = new List<Comment>();
                    foreach (Bookmark bookmark in bookmarkDBAccess.GetUserBookmarks(user.UserID))
                    {
                        results.Add(commentDBAccess.GetComment(bookmark.CommentID));
                    }
                    return results;
                }
            }
            catch (EntityNotFoundException ex)
            {
                // In no reasonable situation is this possible but I'll leave it just in case
                if (ex.EntityType == typeof(Bookmark))
                {
                    throw new FaultException("Bookmark does not exist",
                        new FaultCode("UNKNOWN_BOOKMARK"));
                }
                else // ex.EntityType == typeof(Comment)
                {
                    throw new FaultException("Comment does not exist",
                        new FaultCode("UNKNOWN_COMMENT"));
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Rating AddRating(string sessionToken, int commentID)
        {
            User user = GetCurrentUser(sessionToken);

            try
            {
                using (DBAccess dbAccess = new DBAccess())
                {
                    CommentDBAccess commentDBAccess = new CommentDBAccess(dbAccess);
                    RatingDBAccess ratingDBAccess = new RatingDBAccess(dbAccess);

                    // Test to see if comment exists - will throw exception if not
                    commentDBAccess.GetComment(commentID);

                    // Test to see if rating already exists
                    if (ratingDBAccess.RatingExists(user.UserID, commentID))
                        throw new FaultException("Rating already exists",
                            new FaultCode("DUPLICATE_RATING"));

                    ratingDBAccess.AddRating(user.UserID, commentID, 1);
                    return ratingDBAccess.GetRating(user.UserID, commentID);
                }
            }
            catch (EntityNotFoundException ex)
            {
                if (ex.EntityType == typeof(Comment))
                {
                    throw new FaultException("Comment does not exist",
                        new FaultCode("UNKNOWN_COMMENT"));
                }
                else // ex.EntityType == typeof(Rating)
                {
                    throw new FaultException("Rating could not be added",
                        new FaultCode("RATING_NOT_CREATED"));
                }
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public Rating RemoveRating(string sessionToken, int commentID)
        {
            User user = GetCurrentUser(sessionToken);
            try
            {
                using (RatingDBAccess ratingDBAccess = new RatingDBAccess())
                {
                    Rating rating = ratingDBAccess.GetRating(user.UserID, commentID);
                    ratingDBAccess.RemoveRating(user.UserID, commentID);
                    return rating;
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("Rating could not be removed because it does not exist",
                    new FaultCode("UNKNOWN_RATING"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

        public List<Rating> GetUserRatings(string sessionToken, int? projectID)
        {
            User user = GetCurrentUser(sessionToken);
            try
            {
                using (RatingDBAccess ratingDBAccess = new RatingDBAccess())
                {
                    return ratingDBAccess.GetUserRatings(user.UserID, projectID);
                }
            }
            catch (EntityNotFoundException)
            {
                throw new FaultException("Rating does not exist",
                    new FaultCode("UNKNOWN_RATING"));
            }
            catch (SqlException)
            {
                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));
            }
        }

    }
}
