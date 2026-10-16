using Microsoft.Data.SqlClient;

namespace RaceDay2.Data
{
    public class DatabaseConnectionTest
    {
        private readonly DbConnectionFactory _connectionFactory;

        public DatabaseConnectionTest(
            DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> TestConnectionAsync()
        {
            using SqlConnection connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            return connection.State ==
                   System.Data.ConnectionState.Open;
        }
    }
}