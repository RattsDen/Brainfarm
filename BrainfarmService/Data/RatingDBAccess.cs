using BrainfarmService.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    public class RatingDBAccess : DBAccess
    {

        public RatingDBAccess() : base() { }
        public RatingDBAccess(DBAccess parent) : base(parent) { }

        public Rating GetRating(int userID, int commentID)
        {
            string sql = @"
SELECT CommentID
      ,UserID
      ,Weight
      ,CreationDate
  FROM Rating
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
                        Rating rating = new Rating();
                        rating.CommentID = (int)reader["CommentID"];
                        rating.UserID = (int)reader["UserID"];
                        rating.Weight = (int)reader["Weight"];
                        rating.CreationDate = (DateTime)reader["CreationDate"];
                        return rating;
                    }
                    else
                    {
                        throw new EntityNotFoundException(typeof(Rating));
                    }
                }
            }
        }

        public bool RatingExists(int userID, int commentID)
        {
            string sql = @"
SELECT COUNT(*)
  FROM Rating
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

        public void AddRating(int userID, int commentID, int weight)
        {
            string sql = @"
INSERT INTO Rating
      (CommentID
      ,UserID
      ,Weight
      ,CreationDate)
VALUES(@CommentID
      ,@UserID
      ,@Weight
      ,@CreationDate);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CommentID", commentID);
                command.Parameters.AddWithValue("@Weight", weight);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.ExecuteNonQuery();
            }
        }

        public void RemoveRating(int userID, int commentID)
        {
            string sql = @"
DELETE Rating
 WHERE UserID = @UserID
   AND CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CommentID", commentID);
                command.ExecuteNonQuery();
            }
        }

        public List<Rating> GetUserRatings(int userID, int? projectID)
        {
            List<Rating> results = new List<Rating>();
            string sql = @"
SELECT Rating.CommentID
  FROM Rating
 INNER JOIN Comment
    ON Rating.CommentID = Comment.CommentID
 WHERE Rating.UserID = @UserID
   AND (@ProjectID IS NULL OR ProjectID = @ProjectID)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                if (projectID == null)
                    command.Parameters.AddWithValue("@ProjectID", DBNull.Value);
                else
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int commentID = (int)reader["CommentID"];
                        results.Add(GetRating(userID, commentID));
                    }
                }
            }
            return results;
        }

        public int GetScoreForComment(int commentID)
        {
            // The COALESCE function here ensures that 0 is returned if
            //the comment has no ratings
            //(SUM returns NULL if there are no records)
            string sql = @"
SELECT COALESCE(SUM(Weight), 0)
  FROM Rating
 WHERE CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@CommentID", commentID);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

    }
}