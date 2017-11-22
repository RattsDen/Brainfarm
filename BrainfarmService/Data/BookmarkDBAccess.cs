using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmService.Exceptions;

namespace BrainfarmService.Data
{
    public class BookmarkDBAccess : DBAccess
    {

        public BookmarkDBAccess() : base() { }
        public BookmarkDBAccess(DBAccess parent) : base(parent) { }

        public Bookmark GetBookmark(int userID, int commentID)
        {
            string sql = @"
SELECT UserID
      ,CommentID
      ,CreationDate
  FROM Bookmark
 WHERE UserID = @UserID
   AND CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CommentID", commentID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Bookmark bookmark = new Bookmark();
                        bookmark.UserID = (int)reader["UserID"];
                        bookmark.CommentID = (int)reader["CommentID"];
                        bookmark.CreationDate = (DateTime)reader["CreationDate"];
                        return bookmark;
                    }
                    else
                    {
                        throw new EntityNotFoundException(typeof(Bookmark));
                    }
                }
            }
        }

        public bool BookmarkExists(int userID, int commentID)
        {
            string sql = @"
SELECT COUNT(*)
  FROM Bookmark
 WHERE UserID = @UserID
   AND CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CommentID", commentID);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public void BookmarkComment(int userId, int commentId)
        {
            // -- attempt to insert bookmark record
            string insertSql = @"
INSERT INTO Bookmark
      (UserID
      ,CommentID
      ,CreationDate)
VALUES(@UserID
      ,@CommentID
      ,@CreationDate);
";

            using (SqlCommand command = GetNewCommand(insertSql))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@CommentID", commentId);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }

        public void UnbookmarkComment(int userId, int commentId)
        {
            // -- remove the row from the Bookmark table, to unbookmark the comment
            string deleteRowSql = @"
DELETE Bookmark
 WHERE UserID = @UserID
   AND CommentID = @CommentID";

            using (SqlCommand command = GetNewCommand(deleteRowSql))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@CommentID", commentId);

                command.ExecuteNonQuery();
            }
        }


        // retrieves the commentIds of the comments
        // that the given user has bookmarked for the given project
        public List<int> GetBookmarksForProject(int userId, int projectId)
        {
            List<int> toReturn = new List<int>();
            String sql = @"
SELECT b.CommentID
  FROM Bookmark b, Comment c
 WHERE b.CommentID = c.CommentID
   AND c.ProjectID = @ProjectID
   AND b.UserID = @UserID";


            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectId);
                command.Parameters.AddWithValue("@UserID", userId);

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

        public List<Bookmark> GetUserBookmarks(int userID)
        {
            List<Bookmark> results = new List<Bookmark>();
            string sql = @"
SELECT CommentID
  FROM Bookmark
 WHERE UserID = @UserID
 ORDER BY CreationDate DESC
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("UserID", userID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int commentID = (int)reader["CommentID"];
                        results.Add(GetBookmark(userID, commentID));
                    }
                }
            }
            return results;
        }
    }
}