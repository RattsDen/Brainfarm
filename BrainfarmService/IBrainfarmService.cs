using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BrainfarmService.Data;

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
        bool RegisterUser(string username, string password, string email);

        [OperationContract]
        [WebInvoke]
        string Login(string username, string password, bool keepLoggedIn);

        [OperationContract]
        [WebInvoke]
        User GetCurrentUser(string sessionToken);

        [OperationContract]
        [WebInvoke]
        void Logout(string sessionToken);

        [OperationContract]
        [WebInvoke]
        void CreateProject(string sessionToken, string title, string[] tags, string firstCommentBody);


        
        [OperationContract]
        [WebInvoke]
        void CreateComment(string sessionToken, int projectID, int parentCommentID, 
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification, 
            Dictionary<int, string> synthesizedCommentIDs, Dictionary<string, byte[]> fileUploads);

    }
}
