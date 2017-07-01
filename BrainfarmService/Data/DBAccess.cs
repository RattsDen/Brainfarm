using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BrainfarmService
{
    public class DBAccess : IDisposable
    {
        // Connection string
        private static string connString 
            = ConfigurationManager.ConnectionStrings["BrainfarmDBConnectionString"].ConnectionString;

        private static SqlConnection GetNewConnection()
        {
            return new SqlConnection(connString);
        }

        private SqlConnection conn;
        private SqlTransaction transaction;

        public DBAccess()
        {
            conn = GetNewConnection();
            conn.Open();
        }

        public DBAccess(DBAccess parent)
        {
            conn = parent.conn;
            transaction = parent.transaction;
        }

        public void Dispose()
        {
            if (transaction != null)
                transaction.Dispose();

            conn.Close();
            conn.Dispose();
        }

        public SqlCommand GetNewCommand(string query)
        {
            if (transaction == null)
                return new SqlCommand(query, conn);
            else
                return new SqlCommand(query, conn, transaction);
        }

        public void BeginTransaction()
        {
            transaction = conn.BeginTransaction();
        }

        public void Commit()
        {
            transaction.Commit();
            transaction.Dispose();
            transaction = null;
        }

        public void Rollback()
        {
            transaction.Rollback();
            transaction.Dispose();
            transaction = null;
        }

    }
}