using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmClassLibrary;

namespace BrainfarmService
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

    }
}