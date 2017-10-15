using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmService.Exceptions;

namespace BrainfarmService.Data
{
    /*
     * Class providing database operations for Bookmark entities
     */
    public class BookmarkDBAccess : DBAccess
    {

        public BookmarkDBAccess() : base() { }
        public BookmarkDBAccess(DBAccess parent) : base(parent) { }


        public void BookmarkComment(int userId, int commentId)
        {

            // -- verify that this user didn't already bookmark this comment
            // -- throw an exception if they did
            // -- (better way to do this: catch primary key violation, when you try to insert?)

            string verifySql = @"
SELECT COUNT(*)
  FROM Bookmark
 WHERE userId = @userId
   AND commentId = @commentId";

            using (SqlCommand command = GetNewCommand(verifySql))
            {
                command.Parameters.AddWithValue("@userID", userId);
                command.Parameters.AddWithValue("@commentID", commentId);

                int num = Convert.ToInt32(command.ExecuteScalar());

                if (num != 0)
                {
                    // FIXME
                    Console.WriteLine("Throw 'user already bookmarked this comment' exception");
                    return;   // will throw, instead, when we fix this.
                }
            }


            // -- attempt to insert bookmark record
            try
            {
                string insertSql = @"
INSERT INTO Bookmark(UserID, CommentID, CreationDate)
  VALUES (@userId, @commentId, @creationDate)";

                using (SqlCommand command = GetNewCommand(insertSql))
                {
                    command.Parameters.AddWithValue("@userID", userId);
                    command.Parameters.AddWithValue("@commentID", commentId);
                    command.Parameters.AddWithValue("@creationDate", DateTime.Now);

                    command.ExecuteNonQuery();

                }

            }
            // FIXME: catch, if userId or commentId doesn't exist, ie so that the Foreign Key complains
            catch (SqlException ex)
            {
                Console.WriteLine("hello. this is to help the debugger.");
            }

        }

        public void UnbookmarkComment(int userId, int commentId)
        {
            // -- verify that the user actually bookmarked this comment
            // -- throw an exception if they didn't actually bookmark this comment.

            string verifySql = @"
SELECT COUNT(*)
  FROM Bookmark
 WHERE userId = @userId
   AND commentId = @commentId";

            using (SqlCommand command = GetNewCommand(verifySql))
            {
                command.Parameters.AddWithValue("@userID", userId);
                command.Parameters.AddWithValue("@commentID", commentId);

                int num = Convert.ToInt32(command.ExecuteScalar());

                if (num != 1)
                {
                    // FIXME
                    Console.WriteLine("Throw 'user didn't bookmark this comment' exception");
                    Console.WriteLine("Or, potentially, user, or comment doesn't exist --");
                    Console.WriteLine("(do i need to differentiate this??)");
                    return;   // will throw, instead, when we fix this.
                }
            }

            // -- remove the row from the Bookmark table, to unbookmark the comment
            string deleteRowSql = @"
DELETE
  FROM Bookmark
 WHERE userId = @userId
   AND commentId = @commentId";

            using (SqlCommand command = GetNewCommand(deleteRowSql))
            {
                command.Parameters.AddWithValue("@userID", userId);
                command.Parameters.AddWithValue("@commentID", commentId);

                command.ExecuteNonQuery();
                // FIXME: do i need to catch SQLException?
                // or will it just propogate to the web service method?
            }
        }


        // retrieves the commentIds of the comments
        // that the given user has bookmarked for the given project
        public List<int> GetBookmarksForProject(int userId, int projectId)
        {
            List<int> toReturn = new List<int>();

            // FIXME: what if the userId is invalid? what if the projectId is invalid?
            // should i check for this?

            String sql = @"
SELECT b.CommentId
FROM Bookmark b, Comment c
WHERE b.commentId = c.commentId
  AND c.projectId = @projectId
  AND b.userId = @userId";


            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@projectId", projectId);
                command.Parameters.AddWithValue("@userId", userId);


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int num = reader.GetInt32(reader.GetOrdinal("commentId"));
                        toReturn.Add(num);
                    }
                }

            }

            return toReturn;
        }
    }
}