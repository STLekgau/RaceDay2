
using Microsoft.Data.SqlClient;

namespace RaceDay2.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("RaceDayConnection")
                ?? throw new InvalidOperationException(
                    "RaceDayConnection is missing from appsettings.json.");
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
