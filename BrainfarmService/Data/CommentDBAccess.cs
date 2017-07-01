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
            Dictionary<int, string> syntheses, Dictionary<string, byte[]> fileUploads)
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
                    foreach (KeyValuePair<int, string> synthesis in syntheses)
                    {
                        InsertSynthesisJunction(commentID, synthesis.Key, synthesis.Value);
                    }
                }

                // Insert each contribution file if isContribution == true
                if (isContribution)
                {
                    foreach (KeyValuePair<string, byte[]> upload in fileUploads)
                    {
                        // TODO: Insert contribution files
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
      ,ParrentCommentID
      ,CreationDate
      ,EditedDate
      ,BodyText
      ,IsSynthesis
      ,IsContribution
      ,IsSpecification)
VALUES(@ProjectID
      ,@UserID
      ,@ParrentCommentID
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
                command.Parameters.AddWithValue("@ParrentCommentID", parentCommentID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.Parameters.AddWithValue("@IsSynthesis", isSynthesis);
                command.Parameters.AddWithValue("@IsContribution", isContribution);
                command.Parameters.AddWithValue("@IsSpecification", isSpecification);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void InsertSynthesisJunction(int synthesisCommentID, int linkedCommentID, 
            string summaryBlurb)
        {
            string sql = @"
INSERT INTO SynthesisJunction
      (SynthesisCommentID
      ,LinkedCommentID
      ,SummaryBlurb)
VALUES(@SynthesisCommentID
      ,@LinkedCommentID
      ,@SummaryBlurb)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@SynthesisCommentID", synthesisCommentID);
                command.Parameters.AddWithValue("@LinkedCommentID", linkedCommentID);
                command.Parameters.AddWithValue("@SummaryBlurb", summaryBlurb);
                command.ExecuteNonQuery();
            }
        }
    }
}