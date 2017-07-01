using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService
{
    public static class BrainfarmDBHelper
    {
        // Connection string
        private static readonly string connString 
            = ConfigurationManager.ConnectionStrings["BrainfarmDBConnectionString"].ConnectionString;

        // Get a connection to the BrainfarmDB database
        // The connection will need to be opened still
        public static SqlConnection GetNewConnection()
        {
            return new SqlConnection(connString);
        }

    }
}