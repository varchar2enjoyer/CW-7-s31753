using Microsoft.Data.SqlClient;

namespace CW_7_s31753.Database
{
    public class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException("Connection string not configured");
        }

        public SqlConnection GetConnection() => new(_connectionString);
    }
}