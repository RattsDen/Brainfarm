using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    // Static class to consolidate database access functions for Comments
    public static class CommentDBAccess
    {

        // Insert the first comment on a project that should contain the project description.
        // Takes connection and transaction arguments so that if this part fails, the whole 
        //project posting operation can fail too.
        public static void InsertInitialProjectComment(int projectID, int userID, string bodyText, 
            SqlConnection conn, SqlTransaction trans)
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
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.ExecuteNonQuery();
            }
        }

        public static void CreateComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            Dictionary<int, string> syntheses, Dictionary<string, byte[]> fileUploads)
        {
            // TODO: Implement this method

            // Open connection and transaction
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int commentID = InsertComment(projectID, userID, parentCommentID, 
                            bodyText, isSynthesis, isContribution, isSpecification, 
                            conn, trans);

                        // Insert each synthesis junction if isSynthesis == true
                        if (isSynthesis)
                        {
                            foreach (KeyValuePair<int, string> synthesis in syntheses)
                            {
                                InsertSynthesisJunction(commentID, synthesis.Key, synthesis.Value, 
                                    conn, trans);
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

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
                conn.Close();
            }
        }

        public static int InsertComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SqlConnection conn, SqlTransaction trans)
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
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
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

        public static void InsertSynthesisJunction(int synthesisCommentID, int linkedCommentID, 
            string summaryBlurb, SqlConnection conn, SqlTransaction trans)
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
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
            {
                command.Parameters.AddWithValue("@SynthesisCommentID", synthesisCommentID);
                command.Parameters.AddWithValue("@LinkedCommentID", linkedCommentID);
                command.Parameters.AddWithValue("@SummaryBlurb", summaryBlurb);
                command.ExecuteNonQuery();
            }
        }
    }
}