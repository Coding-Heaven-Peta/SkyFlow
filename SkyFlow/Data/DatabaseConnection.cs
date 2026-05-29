using Microsoft.Data.SqlClient;

namespace SkyFlow.Data
{
    // Provides a centralized way to manage database connections, including connection string and error handling.
    public static class DatabaseConnection
    {
        // use the following connection string for a named SQL Server Express instance:
        private static readonly string _connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SkyFlowDB;Integrated Security=True;Connect Timeout=30;";

        // Returns an open SqlConnection to SkyFlowDB.
        public static SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        //Tests the connection without throwing to the caller.
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
