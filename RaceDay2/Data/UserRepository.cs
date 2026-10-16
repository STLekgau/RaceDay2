
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class UserRepository
    {
        private readonly DbConnectionFactory _factory;

        public UserRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT UserID, FirstName, LastName, Email,
                       PasswordHash, Role, DateRegistered
                FROM Users
                WHERE UserID = @UserID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = id;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                DateRegistered = reader.GetDateTime(
                    reader.GetOrdinal("DateRegistered"))
            };
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = @"
                SELECT UserID, FirstName, LastName, Email,
                       PasswordHash, Role, DateRegistered
                FROM Users
                WHERE Email = @Email";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = email;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                DateRegistered = reader.GetDateTime(
                    reader.GetOrdinal("DateRegistered"))
            };
        }

        public async Task<int> CreateAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users
                    (FirstName, LastName, Email, PasswordHash,
                     Role, DateRegistered)
                OUTPUT INSERTED.UserID
                VALUES
                    (@FirstName, @LastName, @Email, @PasswordHash,
                     @Role, @DateRegistered)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@FirstName", SqlDbType.VarChar, 100)
                .Value = user.FirstName;
            cmd.Parameters.Add("@LastName", SqlDbType.VarChar, 100)
                .Value = user.LastName;
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255)
                .Value = user.Email;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255)
                .Value = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            cmd.Parameters.Add("@Role", SqlDbType.VarChar, 20)
                .Value = user.Role;
            cmd.Parameters.Add("@DateRegistered", SqlDbType.DateTime2)
                .Value = DateTime.UtcNow;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }
}
