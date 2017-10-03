using BrainfarmService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BrainfarmWeb
{
    public partial class TempDevelopingSearchPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSeparateTags_Click(object sender, EventArgs e)
        {
            // -- parse tags [FIXME: put in helper method]
            string projectTagsEnteredString = txtProjectTags.Text.Trim();
            projectTagsEnteredString = System.Text.RegularExpressions.Regex.Replace(projectTagsEnteredString, @"\s+", " ");
            string[] tagsEntered = projectTagsEnteredString.Split(' ');

            // -- display in debug panel
            debugText.Text += "Tags array: <br />";

            for (int x = 0; x < tagsEntered.Length; x++)
            {
                debugText.Text += x + ": " + tagsEntered[x] + "<br />";

            }

            // -- create parameterized placeholders for sql string
            string[] sqlParameterStrings = new string[tagsEntered.Length];
            for (int x = 0; x < tagsEntered.Length; x++)
            {
                sqlParameterStrings[x] = "@" + "A" + x; 
                //^ "A" is there just becuase i'm not sure if an sql parameter is allowed to start with a number
            }

            // -- display in debug panel
            debugText.Text += "<br /><br />";

            for (int x = 0; x < sqlParameterStrings.Length; x++)
            {
                debugText.Text += sqlParameterStrings[x] + "<br />";
            }

            // -- create SQL command and run it.

            string orClauses = "";
            string numberOfTags = sqlParameterStrings.Length.ToString();

            for (int x = 0; x < sqlParameterStrings.Length; x++)
            {
                orClauses += "t.Text = " + sqlParameterStrings[x];
                if (x != (sqlParameterStrings.Length - 1))
                {
                    orClauses += " OR ";
                }
            }
                
            string sqlString = @"
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


            debugText.Text += "<br /><br />" + "<pre>" + sqlString + "</pre>";

            /* (EXAMPLE OF SQL QUERY GENERATED (after parameters are entered) ):
            
SELECT DISTINCT temp.ProjectID, COUNT(*)
FROM 
(
	SELECT p.ProjectID, t.Text
	  FROM Project p, ProjectTag pt, Tag t
	 WHERE p.ProjectID = pt.ProjectID
	   AND pt.tagID = t.tagID
	   AND (t.Text = 'A'
		OR t.Text = 'case')
) temp
GROUP BY temp.ProjectID
HAVING COUNT(*) = 2

            */




            try
            {
                using (BrainfarmService.Data.ProjectDBAccess projectDBAccess = new BrainfarmService.Data.ProjectDBAccess())
                {
                    // return projectDBAccess.GetProject(projectID);
                }
            }
            catch (Exception ex)
            {

            }

            /*
            catch (EntityNotFoundException)
            {

                throw new FaultException("Project could not be found",
                    new FaultCode("UNKNOWN_PROJECT"));

            }
            catch (SqlException)
            {

                throw new FaultException("Error while communicating with database",
                    new FaultCode("DATABASE_ERROR"));

            }
            */

        }

    }
}