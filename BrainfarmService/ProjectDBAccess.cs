using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmClassLibrary;

namespace BrainfarmService
{
    // Static class to consolidate database access functions for Projects
    // Also has some Tag access functions since they are so closely related
    public static class ProjectDBAccess
    {
        public static Project ReadProject(SqlDataReader reader)
        {
            Project project = new Project();
            project.ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectID"));
            project.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            project.Title = reader.GetString(reader.GetOrdinal("Title"));
            project.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            return project;
        }


        public static void CreateProject(int userID, string title, string[] tags, string firstCommentBody)
        {
            using (SqlConnection conn = BrainfarmDBHelper.GetNewConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert project, get its ID
                        int projectID = InsertProject(userID, title, conn, trans);

                        // Get ID for each tag and insert a link between the project and the tag
                        foreach (string tag in tags)
                        {
                            int tagID = GetOrCreateTagID(tag, conn, trans);
                            InsertProjectTag(projectID, tagID, conn, trans);
                        }

                        // Insert inital comment
                        CommentDBAccess.InsertInitialProjectComment(projectID, userID, firstCommentBody, 
                            conn, trans);

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

        public static int InsertProject(int userID, string title, SqlConnection conn, SqlTransaction trans)
        {
            // Insert new project then select its ID.
            string sql = @"
INSERT INTO Project
      (UserID
      ,Title
      ,CreationDate)
VALUES(@UserID
      ,@Title
      ,@CreationDate);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static int GetOrCreateTagID(string tag, SqlConnection conn, SqlTransaction trans)
        {
            // SQL Translation: Insert tag if it doesn't exist already, then get its ID.
            string sql = @"
INSERT INTO Tag ([Text])
SELECT @Text
WHERE NOT EXISTS (SELECT 1 FROM Tag WHERE [Text] = @Text);
SELECT TagID
  FROM Tag
 WHERE Text = @Text;
";
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
            {
                command.Parameters.AddWithValue("@Text", tag);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static void InsertProjectTag(int projectID, int tagID, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
INSERT INTO ProjectTag
      (ProjectID
      ,TagID)
VALUES(@ProjectID
      ,@TagID)
";
            using (SqlCommand command = new SqlCommand(sql, conn, trans))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@TagID", tagID);
                command.ExecuteNonQuery();
            }
        }


    }
}