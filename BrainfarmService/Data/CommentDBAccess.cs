using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    public class CommentDBAccess : DBAccess
    {

        public CommentDBAccess() : base() { }
        public CommentDBAccess(DBAccess parent) : base(parent) { }

        // Insert the first comment on a project that should contain the project description.
        // Takes connection and transaction arguments so that if this part fails, the whole 
        //project posting operation can fail too.
        public void InsertInitialProjectComment(int projectID, int userID, string bodyText)
        {
            string sql = @"
INSERT INTO Comment
      (ProjectID
      ,UserID
      ,ParentCommentID
      ,CreationDate
      ,EditedDate
      ,BodyText
      ,IsSynthesis
      ,IsContribution
      ,IsSpecification)
VALUES(@ProjectID
      ,@UserID
      ,NULL
      ,@CreationDate
      ,NULL
      ,@BodyText
      ,0
      ,0
      ,0)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.ExecuteNonQuery();
            }
        }

        public void CreateComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, string[] fileUploads)
        {
            // TODO: Implement this method

            BeginTransaction();

            try
            {
                int commentID = InsertComment(projectID, userID, parentCommentID, 
                    bodyText, isSynthesis, isContribution, isSpecification);

                // Insert each synthesis junction if isSynthesis == true
                if (isSynthesis)
                {
                    if (syntheses != null)
                    {
                        foreach (SynthesisRequest synthesis in syntheses)
                        {
                            InsertSynthesisJunction(commentID, synthesis.LinkedCommentID, synthesis.Subject);
                        }
                    }
                }

                // Prepare to accept each contribution file if isContribution == true
                if (isContribution)
                {
                    if (fileUploads != null)
                    {
                        foreach (string filename in fileUploads)
                        {
                            // TODO: Register pending upload with FileUploadManager
                        }
                    }
                }

                Commit();
            }
            catch (Exception ex)
            {
                Rollback();
                throw ex;
            }
        }

        public int InsertComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification)
        {
            string sql = @"
INSERT INTO Comment
      (ProjectID
      ,UserID
      ,ParentCommentID
      ,CreationDate
      ,EditedDate
      ,BodyText
      ,IsSynthesis
      ,IsContribution
      ,IsSpecification)
VALUES(@ProjectID
      ,@UserID
      ,@ParentCommentID
      ,@CreationDate
      ,NULL
      ,@BodyText
      ,@IsSynthesis
      ,@IsContribution
      ,@IsSpecification);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ParentCommentID", parentCommentID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.Parameters.AddWithValue("@IsSynthesis", isSynthesis);
                command.Parameters.AddWithValue("@IsContribution", isContribution);
                command.Parameters.AddWithValue("@IsSpecification", isSpecification);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void InsertSynthesisJunction(int synthesisCommentID, int linkedCommentID, 
            string subject)
        {
            string sql = @"
INSERT INTO SynthesisJunction
      (SynthesisCommentID
      ,LinkedCommentID
      ,Subject)
VALUES(@SynthesisCommentID
      ,@LinkedCommentID
      ,@Subject)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@SynthesisCommentID", synthesisCommentID);
                command.Parameters.AddWithValue("@LinkedCommentID", linkedCommentID);
                command.Parameters.AddWithValue("@Subject", subject);
                command.ExecuteNonQuery();
            }
        }

        public List<Comment> GetComments(int projectID, int? parentCommentID)
        {
            List<Comment> results = new List<Comment>();
            string sql = @"
SELECT c.CommentID
      ,c.UserID
      ,c.ParentCommentID
      ,c.CreationDate
      ,c.EditedDate
      ,c.BodyText
      ,c.IsSynthesis
      ,c.IsContribution
      ,c.IsSpecification
      ,u.Username
      ,CASE WHEN (SELECT COUNT(*) 
                    FROM Comment 
                   WHERE ProjectID = @ProjectID 
                     AND ParentCommentID = c.CommentID) > 0 
            THEN 1
            ELSE 0
       END AS HasChildren
  FROM Comment c
 INNER JOIN User u
 WHERE ProjectID = @ProjectID
   AND (ParentCommentID = @ParentCommentID OR (ParentCommentID IS NULL AND @ParentCommentID IS NULL))
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                // Bind parameters
                command.Parameters.AddWithValue("@ProjectID", projectID);
                if (parentCommentID != null)
                    command.Parameters.AddWithValue("@ParentCommentID", parentCommentID);
                else
                    command.Parameters.AddWithValue("@ParentCommentID", DBNull.Value);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    /* TODO: Maybe limit the max number of siblings returned?
                             Doing this would require knowing how to sort them though.
                             It also is only relevant to displaying comments in a 
                             hierarchical list format (compared to say, a web (an idea for later))
                    */
                    while (reader.Read())
                    {
                        // Create the comment object
                        Comment comment = ReadComment(reader);
                        comment.Username = reader.GetString(reader.GetOrdinal("Username"));

                        // If this comment has children...
                        bool hasChildren = reader.GetBoolean(reader.GetOrdinal("HasChildren"));
                        if (hasChildren)
                        {
                            // Get this comment's children - recursive call
                            // TODO: Limit the depth of recursion. Stack overflows are no fun.
                            comment.Children.AddRange(GetComments(projectID, comment.CommentID));
                        }

                        // If comment is a synthesis, get it's links
                        if (comment.IsSynthesis)
                        {
                            // TODO: Get SynthesisJunctions from DB
                        }

                        // If comment is a contribution, get it's files
                        if (comment.IsContribution)
                        {
                            // TODO: Get ContributionFile objects from DB (Just filename, etc. Not the data)
                        }

                        results.Add(comment);
                    }
                }
            }
            return results;
        }

        private Comment ReadComment(SqlDataReader reader)
        {
            Comment comment = new Comment();

            comment.CommentID = reader.GetInt32(reader.GetOrdinal("CommentID"));
            comment.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            if (!reader.IsDBNull(reader.GetOrdinal("ParentCommentID")))
                comment.ParentCommentID = reader.GetInt32(reader.GetOrdinal("ParentCommentID"));
            else
                comment.ParentCommentID = null;
            comment.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            if (!reader.IsDBNull(reader.GetOrdinal("EditedDate")))
                comment.EditedDate = reader.GetDateTime(reader.GetOrdinal("EditedDate"));
            else
                comment.EditedDate = null;
            comment.BodyText = reader.GetString(reader.GetOrdinal("BodyText"));
            comment.IsSynthesis = reader.GetBoolean(reader.GetOrdinal("IsSynthesis"));
            comment.IsContribution = reader.GetBoolean(reader.GetOrdinal("IsContribution"));
            comment.IsSpecification = reader.GetBoolean(reader.GetOrdinal("IsSpecification"));

            return comment;
        }
    }
}