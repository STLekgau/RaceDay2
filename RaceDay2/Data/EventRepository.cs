
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class EventRepository
    {
        private readonly DbConnectionFactory _factory;

        public EventRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        private static Event MapEvent(SqlDataReader reader)
        {
            return new Event
            {
                EventID = reader.GetInt32(reader.GetOrdinal("EventID")),
                OrganiserID = reader.GetInt32(reader.GetOrdinal("OrganiserID")),
                EventName = reader.GetString(reader.GetOrdinal("EventName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null : reader.GetString(reader.GetOrdinal("Description")),
                EventDate = reader.GetDateTime(reader.GetOrdinal("EventDate")),
                Location = reader.GetString(reader.GetOrdinal("Location")),
                Distance = reader.GetDecimal(reader.GetOrdinal("Distance")),
                EventType = reader.GetString(reader.GetOrdinal("EventType")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public async Task<List<Event>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Events ORDER BY EventDate";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            using var reader = await cmd.ExecuteReaderAsync();

            var events = new List<Event>();

            while (await reader.ReadAsync())
                events.Add(MapEvent(reader));

            return events;
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM Events WHERE EventID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapEvent(reader) : null;
        }

        public async Task<int> CreateAsync(Event item)
        {
            const string sql = @"
                INSERT INTO Events
                (OrganiserID, EventName, Description, EventDate,
                 Location, Distance, EventType, CreatedAt)
                OUTPUT INSERTED.EventID
                VALUES
                (@OrganiserID, @EventName, @Description, @EventDate,
                 @Location, @Distance, @EventType, @CreatedAt)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@OrganiserID", SqlDbType.Int)
                .Value = item.OrganiserID;
            cmd.Parameters.Add("@EventName", SqlDbType.VarChar, 150)
                .Value = item.EventName;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)item.Description ?? DBNull.Value;
            cmd.Parameters.Add("@EventDate", SqlDbType.DateTime2)
                .Value = item.EventDate;
            cmd.Parameters.Add("@Location", SqlDbType.VarChar, 200)
                .Value = item.Location;
            cmd.Parameters.Add("@Distance", SqlDbType.Decimal)
                .Value = item.Distance;
            cmd.Parameters["@Distance"].Precision = 8;
            cmd.Parameters["@Distance"].Scale = 2;
            cmd.Parameters.Add("@EventType", SqlDbType.VarChar, 20)
                .Value = item.EventType;
            cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2)
                .Value = DateTime.UtcNow;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(Event item)
        {
            const string sql = @"
                UPDATE Events SET
                    EventName = @EventName,
                    Description = @Description,
                    EventDate = @EventDate,
                    Location = @Location,
                    Distance = @Distance,
                    EventType = @EventType
                WHERE EventID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = item.EventID;
            cmd.Parameters.Add("@EventName", SqlDbType.VarChar, 150)
                .Value = item.EventName;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)item.Description ?? DBNull.Value;
            cmd.Parameters.Add("@EventDate", SqlDbType.DateTime2)
                .Value = item.EventDate;
            cmd.Parameters.Add("@Location", SqlDbType.VarChar, 200)
                .Value = item.Location;
            cmd.Parameters.Add("@Distance", SqlDbType.Decimal)
                .Value = item.Distance;
            cmd.Parameters["@Distance"].Precision = 8;
            cmd.Parameters["@Distance"].Scale = 2;
            cmd.Parameters.Add("@EventType", SqlDbType.VarChar, 20)
                .Value = item.EventType;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Events WHERE EventID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}

