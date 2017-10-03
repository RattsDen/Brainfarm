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

        // populates the searchKeywords array and the sqlParameterStrings array
        // given the searchKeywordsString
        // (note: "out" parameters used, so that the caller can pass in uninitialized string arrays)
        private void HelperMethod_ParseTagsAndCreateSqlParameters(
            string searchKeywordsString, 
            out string[] searchKeywords,
            out string[] sqlParameterStrings)
        {
            // ---- parse tags
            searchKeywordsString = searchKeywordsString.Trim();
            searchKeywordsString = System.Text.RegularExpressions.Regex.Replace(searchKeywordsString, @"\s+", " ");
            searchKeywords = searchKeywordsString.Split(' ');

            
            // ---- create parameterized placeholders for sql string
            sqlParameterStrings = new string[searchKeywords.Length];
            for (int x = 0; x < searchKeywords.Length; x++)
            {
                sqlParameterStrings[x] = "@" + "A" + x;
                //^ "A" is there just becuase i'm not sure if an sql parameter is allowed to start with a number
            }

        }

        // runs the sqlString (using the passed-in sqlParameterStrings and searchKeywords),
        // and populates the list of projects (by adding to the projectList parameter)
        private void HelperMethod_RunSqlAndPopulateProjectIdList (
            string sqlString,
            string[] sqlParameterStrings,
            string[] searchKeywords,
            List<int> projectIdList)
        {


            // -- create SQL command, fill it with parameters, and run it
            // -- to get list of projectIds;

            using (SqlCommand command = GetNewCommand(sqlString))
            {

                for (int x = 0; x < sqlParameterStrings.Length; x++)
                {
                    command.Parameters.AddWithValue(sqlParameterStrings[x], searchKeywords[x]);
                }



                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int projectIdInThisRecord = (Int32)reader["ProjectID"];
                        projectIdList.Add(projectIdInThisRecord);
                    }
                }


            }

        }

        // parameter searchKeywords -- search keywords to use, in one string, separated by spaces
        // (note: if both searchTags and searchTitles is true, then projects that has ALL the titles in
        // only its tags or only its title (but not both) will still be included in the returned list of projects)
        public List<Project> GetProjectsWithAllTheseSearchKeywordsInTagsOrTitle(
            string searchKeywordsString,
            bool searchTags,
            bool searchTitles)
        {



            // -- corner case: if both searchTags and searchTitles is false,
            // return a list with no projects
            if (!searchTags && !searchTitles)
            {
                return new List<Project>();
            }

            // -- corner case: if the parameter is null, return a list with no projects
            if (searchKeywordsString == null)
            {
                return new List<Project>();
            }

            // -- corner case: if no search keywords were given, return a list with no projects
            if (searchKeywordsString.Trim().Equals(""))
            {
                return new List<Project>();
            }

            // split the searchKeyword strings into an array of keywords,
            // and create an array of parameters for the sql string, for those keywords
            string[] searchKeywords;
            string[] sqlParameterStrings;

            // populate searchKeywords and sqlParameterStrings arrays
            HelperMethod_ParseTagsAndCreateSqlParameters(
                searchKeywordsString,
                out searchKeywords,
                out sqlParameterStrings);

            // ---- get projectIds, for search by tags, and search by title,
            // ---- doing each search separately

            List<int> projectIdsForTagSearch = new List<int>();
            List<int> projectIdsForTitleSearch = new List<int>();
            
            if (searchTags)
            {
                // -- create SQL string for tags

                string orClauses = "";
                string numberOfTags = sqlParameterStrings.Length.ToString();

                for (int x = 0; x < sqlParameterStrings.Length; x++)
                {
                    orClauses += "UPPER(t.Text) = UPPER(" + sqlParameterStrings[x] + ")";
                    if (x != (sqlParameterStrings.Length - 1))
                    {
                        orClauses += " OR ";
                    }
                }

                string sqlStringForTagSearch = @"
SELECT DISTINCT temp.ProjectID, COUNT(*)
FROM
(
    SELECT p.ProjectID, t.Text
      FROM Project p, ProjectTag pt, Tag t
     WHERE p.ProjectID = pt.ProjectID
       AND pt.tagID = t.tagID
       AND (" + orClauses + @")
) temp
GROUP BY temp.ProjectID
HAVING COUNT(*) = " + numberOfTags;


                // -- run the SQL string to fill in a list of projectIds
                HelperMethod_RunSqlAndPopulateProjectIdList(
                    sqlStringForTagSearch, sqlParameterStrings, searchKeywords, projectIdsForTagSearch);

            }   // from "if (searchTags)"


            if (searchTitles)
            {
                // -- create SQL string

                string andClauses = "";

                for (int x = 0; x < sqlParameterStrings.Length; x++)
                {

                    andClauses += "UPPER(p.Title) LIKE UPPER(CONCAT('%', " + sqlParameterStrings[x] + ", '%')) ";
                    // ^ (CONACT needed, otherwise the parameter not recognized when surrounded by % characters)

                    if (x != (sqlParameterStrings.Length - 1))
                    {
                        andClauses += " AND ";
                    }
                }

                string sqlStringForTitleSearch = @"
SELECT DISTINCT p.ProjectID
  FROM Project p
 WHERE (" + andClauses + ")";

            // -- run the SQL string to fill in a list of projectIds
            HelperMethod_RunSqlAndPopulateProjectIdList(
                sqlStringForTitleSearch, sqlParameterStrings, searchKeywords, projectIdsForTitleSearch);


            }  // from if (searchTitles)


            // --- merge the two list of project Ids
            List<int> projectIdList;
            projectIdList = projectIdsForTagSearch;
            foreach (int projectIdElement in projectIdsForTitleSearch)
            {
                if (!projectIdList.Contains(projectIdElement))
                {
                    projectIdList.Add(projectIdElement);
                }
            }

            // ---- return list of project objects
            List<Project> toReturn = new List<Project>();

            foreach (int projectIdElement in projectIdList)
            {
                toReturn.Add(GetProject(projectIdElement));
            }

            return toReturn;


        }

    }




    



}  // for namespace