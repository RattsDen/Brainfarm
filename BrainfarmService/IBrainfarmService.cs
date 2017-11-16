using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BrainfarmService.Data;
using System.IO;

namespace BrainfarmService
{
    /*
     * This interface declares all of the methods of this service that 
     * can be accessed by another application/process.
     * To make a method be accessable from any other process such as the
     * ASP.NET web server, Javascript AJAX calls, or mobile Java app 
     * it's method signiature must be listed here.
     * 
     * This interface is kind of like the .h file of a C library
     * 
     * Generally you will want to mark methods here with the following
     * two attributes.
     * 
     * [OperationContract]  This attribute is what makes a method
     *                      accessable from another application
     *                      (makes it part of the service)
     * 
     * [WebInvoke]          This attribute allows the method to be called
     *                      using http POST requests
     * 
     */

    [ServiceContract]  // This attribute marks the interface as containing service methods
    public interface IBrainfarmService
    {
        // This method is pretty much just for connectivity testing
        [OperationContract]
        [WebInvoke]
        string GetTimestamp();

        [OperationContract]
        [WebInvoke]
        User RegisterUser(string username, string password, string email);

        [OperationContract]
        [WebInvoke]
        User UpdateUserEmail(string sessionToken, string newEmail);

        [OperationContract]
        [WebInvoke]
        User ChangePassword(string sessionToken, string oldPassword, string newPassword);

        [OperationContract]
        [WebInvoke]
        string Login(string username, string password, bool keepLoggedIn);

        [OperationContract]
        [WebInvoke]
        string RenewToken(string sessionToken);

        [OperationContract]
        [WebInvoke]
        User GetCurrentUser(string sessionToken);

        // Logging out using JWTs simply means throwing away your token
        //[OperationContract]
        //[WebInvoke]
        //void Logout(string sessionToken);

        [OperationContract]
        [WebInvoke]
        Project CreateProject(string sessionToken, string title, string[] tags, string firstCommentBody);
        
        [OperationContract]
        [WebInvoke]
        Project GetProject(int projectID);
        
        [OperationContract]
        [WebInvoke]
        Comment CreateComment(string sessionToken, int projectID, int parentCommentID, 
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, FileAttachmentRequest[] attachments);

        /// <summary>
        /// Edit an existing comment.
        /// </summary>
        /// <param name="sessionToken">Token identifying current user. Obtained from Login method.</param>
        /// <param name="commentID">ID of comment to edit</param>
        /// <param name="bodyText">New body text of comment</param>
        /// <param name="isSynthesis">Bool specifying if comment is a synthesis</param>
        /// <param name="isContribution">Bool specifying if comment is a contribution</param>
        /// <param name="isSpecification">Bool specifying if comment is a specification</param>
        /// <param name="syntheses">Array of SynthesisRequest objects specifying links to other comments</param>
        /// <param name="attachments">
        /// Array of FileAttachmentRequest objects specifying contribution files to be attached to this comment. 
        /// Pass null or an empty array to leave file attachments unchanged.
        /// </param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke]
        Comment EditComment(string sessionToken, int commentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, FileAttachmentRequest[] attachments);

        [OperationContract]
        [WebInvoke]
        int RemoveComment(string sessionToken, int commentID);

        [OperationContract]
        [WebInvoke]
        ContributionFile UploadFile(Stream stream);

        [OperationContract]
        [WebInvoke]
        Stream DownloadFile(int contributionFileID);

        [OperationContract]
        [WebInvoke]
        List<Comment> GetComments(int projectID, int? parentCommentID);

        [OperationContract]
        [WebInvoke]
        List<Project> GetPopularProjects(int top);

        [OperationContract]
        [WebInvoke]
        List<Project> GetRecommendedProjects(int userID, int top);

        [OperationContract]
        [WebInvoke]
        List<Project> GetUserProjects(int userID);

        [OperationContract]
        [WebInvoke]
        List<Comment> GetUserComments(int userID);

        [OperationContract]
        [WebInvoke]
        List<Project> SearchProjects(string searchKeywordsString, bool searchTags, bool searchTitles);

        [OperationContract]
        [WebInvoke]
        void BookmarkComment(string sessionToken, int commentID);

        [OperationContract]
        [WebInvoke]
        void UnbookmarkComment(string sessionToken, int commentID);

        [OperationContract]
        [WebInvoke]
        List<int> GetBookmarksForProject(string sessionToken, int projectID);

    }
}
