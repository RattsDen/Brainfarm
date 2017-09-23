using BrainfarmService.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace BrainfarmService.Data
{
    public class ContributionFileDBAccess : DBAccess
    {

        public ContributionFileDBAccess() : base() { }
        public ContributionFileDBAccess(DBAccess parent) : base(parent) { }

        public ContributionFile InsertContributionFile(Stream stream)
        {
            string sql = @"
INSERT INTO ContributionFile
      (Data)
VALUES(@Data);
SELECT SCOPE_IDENTITY();
";
            int contributionFileID;

            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@Data", stream);
                // Execute query and store id of new entity
                contributionFileID = Convert.ToInt32(command.ExecuteScalar());
            }

            // Return the newly created entity
            return GetContibutionFile(contributionFileID);
        }

        public Stream GetFileContents(int contributionFileID)
        {
            string sql = @"
SELECT Data
  FROM ContributionFile
 WHERE ContributionFileID = @ContributionFIleID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ContributionFIleID", contributionFileID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetStream(reader.GetOrdinal("Data"));
                    else
                        throw new EntityNotFoundException();
                }
            }
        }

        public ContributionFile GetContibutionFile(int contributionFileID)
        {
            string sql = @"
SELECT ContributionFileID
      ,CommentID
      ,Filename
  FROM ContributionFile
 WHERE ContributionFileID = @ContributionFIleID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ContributionFIleID", contributionFileID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return ReadContributionFile(reader);
                    else
                        throw new EntityNotFoundException();
                }
            }
        }

        private ContributionFile ReadContributionFile(SqlDataReader reader)
        {
            ContributionFile contributionFile = new ContributionFile();
            contributionFile.ContributionFileID = reader.GetInt32(reader.GetOrdinal("ContributionFileID"));
            if (!reader.IsDBNull(reader.GetOrdinal("CommentID")))
                contributionFile.CommentID = reader.GetInt32(reader.GetOrdinal("CommentID"));
            if (!reader.IsDBNull(reader.GetOrdinal("Filename")))
                contributionFile.Filename = reader.GetString(reader.GetOrdinal("Filename"));
            return contributionFile;
        }

        // Attach an already uploaded file to a comment
        public void AttachFileToComment(int contributionFileID, int commentID, string filename)
        {
            string sql = @"
UPDATE ContributionFile
   SET CommentID = @CommentID
      ,Filename = @Filename
 WHERE ContributionFileID = @ContributionFileID
   AND CommentID IS NULL
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@ContributionFileID", contributionFileID);
                command.Parameters.AddWithValue("@CommentID", commentID);
                command.Parameters.AddWithValue("@Filename", filename);

                int rowsAffected = command.ExecuteNonQuery();

                // Throw exception if nothing was updated
                if (rowsAffected == 0)
                    throw new EntityNotFoundException();
            }
        }

        public List<ContributionFile> GetFilesForComment(int commentID)
        {
            List<ContributionFile> contributionFiles = new List<ContributionFile>();
            string sql = @"
SELECT ContributionFileID
      ,CommentID
      ,Filename
  FROM ContributionFile
 WHERE CommentID = @CommentID
";
            using (SqlCommand command = GetNewCommand(sql))
            {
                command.Parameters.AddWithValue("@CommentID", commentID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contributionFiles.Add(ReadContributionFile(reader));
                    }
                }
            }

            return contributionFiles;
        }

    }
}