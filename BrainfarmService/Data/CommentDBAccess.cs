using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using BrainfarmService.Exceptions;

namespace BrainfarmService.Data
{
    public class CommentDBAccess : DBAccess
    {

        public CommentDBAccess() : base() { }
        public CommentDBAccess(DBAccess parent) : base(parent) { }

        // Insert the first comment on a project that should contain the project description.
        // Takes connection and transaction arguments so that if this part fails, the whole 
        //project posting operation can fail too.
        public void InsertInitialProjectComment(int projectID, int userID, string bodyText)
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
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.ExecuteNonQuery();
            }
        }

        public int CreateComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification,
            SynthesisRequest[] syntheses, FileAttachmentRequest[] attachments)
        {
            // TODO: Implement this method

            BeginTransaction();

            try
            {
                int commentID = InsertComment(projectID, userID, parentCommentID, 
                    bodyText, isSynthesis, isContribution, isSpecification);

                // Insert each synthesis junction if isSynthesis == true
                if (isSynthesis)
                {
                    if (syntheses != null)
                    {
                        foreach (SynthesisRequest synthesis in syntheses)
                        {
                            InsertSynthesisJunction(commentID, synthesis.LinkedCommentID, synthesis.Subject);
                        }
                    }
                }

                // Prepare to accept each contribution file if isContribution == true
                if (isContribution)
                {
                    if (attachments != null)
                    {
                        /* 
                         * This DB Access object does not need to be wrapped in a using block
                         * because it shares the database connection with this instance of
                         * CommentDBAccess. (By using the overloaded constructor) The
                         * connection will be closed when this CommentDBAccess instance is
                         * disposed
                        */
                        ContributionFileDBAccess contributionFileDBAccess 
                            = new ContributionFileDBAccess(this);

                        foreach (FileAttachmentRequest attachment in attachments)
                        {
                            contributionFileDBAccess.AttachFileToComment(
                                attachment.ContributionFileID, 
                                commentID, 
                                attachment.Filename);
                        }
                    }
                }

                Commit();
                return commentID;
            }
            catch (Exception ex)
            {
                Rollback();
                throw ex;
            }
        }

        //returns number of rows affected by update
        public int EditComment(int commentID, int userID, 
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification)
        {
            string sql = @"
UPDATE Comment SET 
BodyText = @BodyText,
EditedDate = @EditedDate,
IsSynthesis = @IsSynthesis,
IsContribution = @IsContribution,
IsSpecification = @IsSpecification 
WHERE CommentID = @CommentID AND UserID = @UserID;
";

            using(SqlCommand command = GetNewCommand(sql)){
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.Parameters.AddWithValue("@EditedDate", DateTime.Now);
                command.Parameters.AddWithValue("@IsSynthesis", isSynthesis);
                command.Parameters.AddWithValue("@IsContribution", isContribution);
                command.Parameters.AddWithValue("@IsSpecification", isSpecification);
                command.Parameters.AddWithValue("@CommentID", commentID);
                command.Parameters.AddWithValue("@UserID", userID);

                return Convert.ToInt32(command.ExecuteNonQuery());
            }
        }

        public int InsertComment(int projectID, int userID, int parentCommentID,
            string bodyText, bool isSynthesis, bool isContribution, bool isSpecification)
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
      ,@ParentCommentID
      ,@CreationDate
      ,NULL
      ,@BodyText
      ,@IsSynthesis
      ,@IsContribution
      ,@IsSpecification);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ProjectID", projectID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ParentCommentID", parentCommentID);
                command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                command.Parameters.AddWithValue("@BodyText", bodyText);
                command.Parameters.AddWithValue("@IsSynthesis", isSynthesis);
                command.Parameters.AddWithValue("@IsContribution", isContribution);
                command.Parameters.AddWithValue("@IsSpecification", isSpecification);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int RemoveComment(int commentID, int userID)
        {
            string sql = @"
UPDATE Comment SET
BodyText = @BodyText,
EditedDate = @EditedDate,
IsRemoved = 1
WHERE CommentID = @CommentID AND UserID = @UserID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@BodyText", "[Comment Removed]");
                command.Parameters.AddWithValue("@EditedDate", DateTime.Now);
                command.Parameters.AddWithValue("@CommentID", commentID);
                command.Parameters.AddWithValue("@UserID", userID);

                return Convert.ToInt32(command.ExecuteNonQuery());
            }
        }

        public void InsertSynthesisJunction(int synthesisCommentID, int linkedCommentID, 
            string subject)
        {
            string sql = @"
INSERT INTO SynthesisJunction
      (SynthesisCommentID
      ,LinkedCommentID
      ,Subject)
VALUES(@SynthesisCommentID
      ,@LinkedCommentID
      ,@Subject)
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@SynthesisCommentID", synthesisCommentID);
                command.Parameters.AddWithValue("@LinkedCommentID", linkedCommentID);
                command.Parameters.AddWithValue("@Subject", subject);
                command.ExecuteNonQuery();
            }
        }

        public List<Comment> GetComments(int projectID, int? parentCommentID)
        {
            List<Comment> results = new List<Comment>();
            List<Comment> commentsWithChildren = new List<Comment>();
            List<Comment> contributionComments = new List<Comment>();
            List<Comment> synthesisComments = new List<Comment>();

            string sql = @"
SELECT c.CommentID
      ,c.UserID
      ,c.ProjectID
      ,c.ParentCommentID
      ,c.CreationDate
      ,c.EditedDate
      ,c.BodyText
      ,c.IsSynthesis
      ,c.IsContribution
      ,c.IsSpecification
      ,c.IsRemoved
      ,u.Username
      ,CASE WHEN (SELECT COUNT(*) 
                    FROM Comment 
                   WHERE ProjectID = @ProjectID 
                     AND ParentCommentID = c.CommentID) > 0 
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
       END AS HasChildren
  FROM Comment c
 INNER JOIN [User] u
    ON c.UserID = u.UserID
 WHERE ProjectID = @ProjectID
   AND (ParentCommentID = @ParentCommentID OR (ParentCommentID IS NULL AND @ParentCommentID IS NULL))
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                // Bind parameters
                command.Parameters.AddWithValue("@ProjectID", projectID);
                if (parentCommentID != null)
                    command.Parameters.AddWithValue("@ParentCommentID", parentCommentID);
                else
                    command.Parameters.AddWithValue("@ParentCommentID", DBNull.Value);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    /* TODO: Maybe limit the max number of siblings returned?
                             Doing this would require knowing how to sort them though.
                             It also is only relevant to displaying comments in a 
                             hierarchical list format (compared to say, a web (an idea for later))
                    */
                    while (reader.Read())
                    {
                        // Create the comment object
                        Comment comment = ReadComment(reader);
                        comment.Username = reader.GetString(reader.GetOrdinal("Username"));

                        // If this comment has children...
                        bool hasChildren = reader.GetBoolean(reader.GetOrdinal("HasChildren"));
                        if (hasChildren)
                        {
                            // Since we can't open more than one reader for the same query on the 
                            //same connection, add this comment to a list so we can remember to get
                            //its children once this tier has finished being read.
                            commentsWithChildren.Add(comment);

                            // TODO: Limit the depth of recursion. Stack overflows are no fun.
                        }

                        // If comment is a synthesis, get it's links
                        if (comment.IsSynthesis)
                        {
                            comment.Syntheses = GetSyntheses(comment.CommentID);
                        }

                        if (comment.IsContribution)
                        {
                            // Remember this comment so we can get its files once the reader is closed.
                            // See above comments about children
                            contributionComments.Add(comment);
                        }

                        results.Add(comment);
                    }
                }
            }

            foreach (Comment comment in commentsWithChildren)
            {
                // Recursivly call this method to get the children of any comments that have children
                comment.Children.AddRange(GetComments(projectID, comment.CommentID));
            }

            // Get ContributionFile objects from DB (Just filename, etc. Not the data)
            /* 
             * This DB Access object does not need to be wrapped in a using block
             * because it shares the database connection with this instance of
             * CommentDBAccess. (By using the overloaded constructor) The
             * connection will be closed when this CommentDBAccess instance is
             * disposed
             */
            ContributionFileDBAccess contributionFileDBAccess = new ContributionFileDBAccess(this);
            foreach (Comment comment in contributionComments)
            {
                comment.ContributionFiles.AddRange(
                    contributionFileDBAccess.GetFilesForComment(comment.CommentID));
            }

            return results;
        }

        public Comment GetComment(int commentId)
        {
            Comment result = new Comment();

            string sql = @"
SELECT c.CommentID
      ,c.UserID
      ,c.ProjectID
      ,c.ParentCommentID
      ,c.CreationDate
      ,c.EditedDate
      ,c.BodyText
      ,c.IsSynthesis
      ,c.IsContribution
      ,c.IsSpecification
      ,c.IsRemoved
      ,u.Username
  FROM Comment c
 INNER JOIN [User] u
    ON c.UserID = u.UserID
 WHERE c.CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@CommentID", commentId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = ReadComment(reader);
                        result.Username = reader.GetString(reader.GetOrdinal("Username"));
                    }
                    else
                    {
                        throw new EntityNotFoundException();
                    }
                }
            }
            return result;
        }

        private Comment ReadComment(SqlDataReader reader)
        {
            Comment comment = new Comment();

            comment.CommentID = reader.GetInt32(reader.GetOrdinal("CommentID"));
            comment.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            comment.ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectID"));
            if (!reader.IsDBNull(reader.GetOrdinal("ParentCommentID")))
                comment.ParentCommentID = reader.GetInt32(reader.GetOrdinal("ParentCommentID"));
            else
                comment.ParentCommentID = null;
            comment.CreationDate = reader.GetDateTime(reader.GetOrdinal("CreationDate"));
            if (!reader.IsDBNull(reader.GetOrdinal("EditedDate")))
                comment.EditedDate = reader.GetDateTime(reader.GetOrdinal("EditedDate"));
            else
                comment.EditedDate = null;
            comment.BodyText = reader.GetString(reader.GetOrdinal("BodyText"));
            comment.IsSynthesis = reader.GetBoolean(reader.GetOrdinal("IsSynthesis"));
            comment.IsContribution = reader.GetBoolean(reader.GetOrdinal("IsContribution"));
            comment.IsSpecification = reader.GetBoolean(reader.GetOrdinal("IsSpecification"));
            comment.IsRemoved = reader.GetBoolean(reader.GetOrdinal("IsRemoved"));

            return comment;
        }

        public List<Comment> GetUserComments(int userID)
        {
            List<Comment> results = new List<Comment>();
            string sql = @"
SELECT c.CommentID
      ,c.UserID
      ,c.ProjectID
      ,c.ParentCommentID
      ,c.CreationDate
      ,c.EditedDate
      ,c.BodyText
      ,c.IsSynthesis
      ,c.IsContribution
      ,c.IsSpecification
      ,c.IsRemoved
      ,u.Username
  FROM Comment c
 INNER JOIN [User] u
    ON c.UserID = u.UserID
 WHERE c.UserID = @UserID
 ORDER BY CreationDate DESC
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Comment comment = ReadComment(reader);
                        comment.Username = reader.GetString(reader.GetOrdinal("Username"));
                        // Exclude synthesis and contribution file lists to make
                        //it clearer that they are not included in these results
                        comment.Syntheses = null;
                        comment.ContributionFiles = null;

                        results.Add(comment);
                    }
                }
            }

            return results;
        }


        public List<SynthesisJunction> GetSyntheses(int synthesisCommentId)
        {
            List<SynthesisJunction> syntheses = new List<SynthesisJunction>();

            String sql = @"
SELECT LinkedCommentID,
Subject
FROM SynthesisJunction
WHERE SynthesisCommentID = @SynthesisCommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@SynthesisCommentID", synthesisCommentId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SynthesisJunction synth = new SynthesisJunction();
                        synth.SynthesisCommentID = synthesisCommentId;
                        synth.LinkedCommentID = reader.GetInt32(reader.GetOrdinal("LinkedCommentID"));
                        synth.Subject = reader.GetString(reader.GetOrdinal("Subject"));

                        syntheses.Add(synth);
                    }
                }
            }

            return syntheses;
        }
    }

}