
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class EventCategoryRepository
    {
        private readonly DbConnectionFactory _factory;

        public EventCategoryRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<EventCategory>> GetByEventAsync(int eventId)
        {
            const string sql = @"
                SELECT EventCategoryID, EventID, CategoryID
                FROM EventCategories
                WHERE EventID = @EventID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@EventID", SqlDbType.Int).Value = eventId;

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<EventCategory>();

            while (await reader.ReadAsync())
            {
                items.Add(new EventCategory
                {
                    EventCategoryID = reader.GetInt32(
                        reader.GetOrdinal("EventCategoryID")),
                    EventID = reader.GetInt32(reader.GetOrdinal("EventID")),
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"))
                });
            }

            return items;
        }

        public async Task<int> AddAsync(int eventId, int categoryId)
        {
            const string sql = @"
                INSERT INTO EventCategories (EventID, CategoryID)
                OUTPUT INSERTED.EventCategoryID
                VALUES (@EventID, @CategoryID)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@EventID", SqlDbType.Int).Value = eventId;
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> RemoveAsync(int eventId, int categoryId)
        {
            const string sql = @"
                DELETE FROM EventCategories
                WHERE EventID = @EventID AND CategoryID = @CategoryID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@EventID", SqlDbType.Int).Value = eventId;
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
