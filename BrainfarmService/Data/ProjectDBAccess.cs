using BrainfarmService.Exceptions;
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

        public int CreateProject(int userID, string title, string[] tags, string firstCommentBody)
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
                return projectID;
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

        public Project GetProject(int projectID)
        {
            Project project;
            string sql = @"
SELECT p.ProjectID
      ,p.UserID
      ,p.Title
      ,p.CreationDate
      ,u.Username
  FROM Project p
 INNER JOIN [User] u
    ON p.UserID = u.UserID
 WHERE p.ProjectID = @ProjectID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        project = ReadProject(reader);
                        project.Username = reader.GetString(reader.GetOrdinal("Username"));
                    }
                    else
                    {
                        throw new EntityNotFoundException();
                    }
                }
            }

            project.Tags.AddRange(GetProjectTags(projectID));

            return project;
        }

        public List<string> GetProjectTags(int projectID)
        {
            List<string> tags = new List<string>();
            string sql = @"
SELECT Text
  FROM Tag t
 INNER JOIN ProjectTag pt
    ON pt.TagID = t.TagID
 WHERE pt.ProjectID = @ProjectID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tagText = reader.GetString(reader.GetOrdinal("Text"));
                        tags.Add(tagText);
                    }
                }
            }
            return tags;
        }

        public List<Project> GetPopularProjects(int top)
        {
            List<Project> results = new List<Project>();
            List<int> projectIDs = new List<int>();

            // List of project IDs sorted by weighted comment creation rate
            // Let x = number of days since comment was created
            // Sort by 1 / 2^x
            // This puts more weight on recent comments
            string sql = @"
SELECT TOP (@Top)
       p.ProjectID
	  ,SUM(
	     1 / CAST(POWER(2, (DATEDIFF(DAY, c.CreationDate, @Today) + 1)) AS DECIMAL)
	   ) AS Activity
  FROM Project p
 INNER JOIN Comment c
    ON p.ProjectID = c.ProjectID
 WHERE DATEDIFF(DAY, c.CreationDate, @Today) <= 30
 GROUP BY p.ProjectID
 ORDER BY Activity DESC
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Top", top);
                command.Parameters.AddWithValue("@Today", DateTime.Today);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        projectIDs.Add(reader.GetInt32(reader.GetOrdinal("ProjectID")));
                    }
                }
            }

            foreach (int projectID in projectIDs)
            {
                results.Add(GetProject(projectID));
            }

            return results;
        }

        public List<Project> GetRecommendedProjects(int userID, int top)
        {
            List<Project> results = new List<Project>();
            List<int> projectIDs = new List<int>();

            /* 
             * Get the list of all tags from all projects that the user has posted in
             * A tag's weight will be the number of times it occurs in this list
             * If a project contains a tag from this list, it will gain that tag's weight
             *   (If tag ALPHA has a weight of 2, and tag BETA has a weight of 3, a project
             *   tagged with both ALPHA and BETA will have a total weight of 5)
             * Order results by weight, descending
             * Lastly, exclude any project the user has already posted in
             */
            string sql = @"
SELECT TOP (@Top)
       pt.ProjectID
      ,COUNT(*)
  FROM ProjectTag pt
 INNER JOIN (
			 SELECT t.TagID
			       ,t.Text
			   FROM ProjectTag pt
			  INNER JOIN Tag t
			     ON pt.TagID = t.TagID
			  INNER JOIN (
			 			 SELECT DISTINCT p.ProjectID
			 			   FROM Project p
			 			  INNER JOIN Comment c
			 			     ON c.ProjectID = p.ProjectID
			 			  WHERE c.UserID = @UserID
			 			) up
			     ON pt.ProjectID = up.ProjectID
			 ) ut
    ON pt.TagID = ut.TagID
 INNER JOIN Project p
    ON pt.ProjectID = p.ProjectID
 WHERE p.ProjectID NOT IN (
                           SELECT DISTINCT p.ProjectID
			 			     FROM Project p
			 			    INNER JOIN Comment c
			 			       ON c.ProjectID = p.ProjectID
			 			    WHERE c.UserID = @UserID
						  )
 GROUP BY pt.ProjectID
 ORDER BY COUNT(*) DESC
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Top", top);
                command.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        projectIDs.Add(reader.GetInt32(reader.GetOrdinal("ProjectID")));
                    }
                }
            }

            foreach (int projectID in projectIDs)
            {
                results.Add(GetProject(projectID));
            }

            return results;
        }
    }
}