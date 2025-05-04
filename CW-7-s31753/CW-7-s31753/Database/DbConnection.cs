using Microsoft.Data.SqlClient;

namespace CW_7_s31753.Database
{
    public class DbConnection
    {
        private readonly string _connectionString;
        
        public DbConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public SqlConnection GetConnection() => new SqlConnection(_connectionString);
    }
}