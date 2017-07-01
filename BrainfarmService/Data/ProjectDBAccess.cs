using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    // Static class to consolidate database access functions for Projects
    // Also has some Tag access functions since they are so closely related
    public class ProjectDBAccess : DBAccess
    {
        public ProjectDBAccess() : base() { }
        public ProjectDBAccess(DBAccess parent) : base(parent) { }

        public static Project ReadProject(SqlDataReader reader)
        {
            Project project = new Project();
            project.ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectID"));
            project.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            project.Title = reader.GetString(reader.GetOrdinal("Title"));
            project.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            return project;
        }

        public void CreateProject(int userID, string title, string[] tags, string firstCommentBody)
        {
            BeginTransaction();
            try
            {
                // Insert project, get its ID
                int projectID = InsertProject(userID, title);

                // Get ID for each tag and insert a link between the project and the tag
                foreach (string tag in tags)
                {
                    int tagID = GetOrCreateTagID(tag);
                    InsertProjectTag(projectID, tagID);
                }

                // Insert inital comment
                CommentDBAccess commentDBAccess = new CommentDBAccess(this);
                commentDBAccess.InsertInitialProjectComment(projectID, userID, firstCommentBody);

                Commit();
            }
            catch (Exception ex)
            {
                Rollback();
                throw ex;
            }
        }

        public int InsertProject(int userID, string title)
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
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int GetOrCreateTagID(string tag)
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
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Text", tag);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void InsertProjectTag(int projectID, int tagID)
        {
            string sql = @"
INSERT INTO ProjectTag
      (ProjectID
      ,TagID)
VALUES(@ProjectID
      ,@TagID)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@TagID", tagID);
                command.ExecuteNonQuery();
            }
        }


    }
}