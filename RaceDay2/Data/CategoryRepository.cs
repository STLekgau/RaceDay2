
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class CategoryRepository
    {
        private readonly DbConnectionFactory _factory;

        public CategoryRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Categories ORDER BY CategoryName";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            using var reader = await cmd.ExecuteReaderAsync();

            var categories = new List<Category>();

            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                    CategoryName = reader.GetString(
                        reader.GetOrdinal("CategoryName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description"))
                });
            }

            return categories;
        }

        public async Task<int> CreateAsync(Category category)
        {
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description)
                OUTPUT INSERTED.CategoryID
                VALUES (@Name, @Description)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 100)
                .Value = category.CategoryName;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)category.Description ?? DBNull.Value;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql =
                "DELETE FROM Categories WHERE CategoryID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
