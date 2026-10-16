
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class EnrollmentRepository
    {
        private readonly DbConnectionFactory _factory;

        public EnrollmentRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<Enrollment>> GetByParticipantAsync(int participantId)
        {
            const string sql = @"
                SELECT EnrollmentID, ParticipantID, EventID,
                       CategoryID, EnrollmentDate
                FROM Enrollments
                WHERE ParticipantID = @ID
                ORDER BY EnrollmentDate DESC";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = participantId;

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Enrollment>();

            while (await reader.ReadAsync())
                items.Add(MapEnrollment(reader));

            return items;
        }

        public async Task<List<Enrollment>> GetByEventAsync(int eventId)
        {
            const string sql = @"
                SELECT EnrollmentID, ParticipantID, EventID,
                       CategoryID, EnrollmentDate
                FROM Enrollments
                WHERE EventID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = eventId;

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Enrollment>();

            while (await reader.ReadAsync())
                items.Add(MapEnrollment(reader));

            return items;
        }

        // NEW: used by EnrollmentsController.Delete to check who owns an enrollment.
        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT EnrollmentID, ParticipantID, EventID,
                       CategoryID, EnrollmentDate
                FROM Enrollments
                WHERE EnrollmentID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapEnrollment(reader) : null;
        }

        private static Enrollment MapEnrollment(SqlDataReader reader)
        {
            return new Enrollment
            {
                EnrollmentID = reader.GetInt32(reader.GetOrdinal("EnrollmentID")),
                ParticipantID = reader.GetInt32(reader.GetOrdinal("ParticipantID")),
                EventID = reader.GetInt32(reader.GetOrdinal("EventID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                EnrollmentDate = reader.GetDateTime(
                    reader.GetOrdinal("EnrollmentDate"))
            };
        }

        public async Task<int> CreateAsync(Enrollment enrollment)
        {
            const string sql = @"
                INSERT INTO Enrollments
                    (ParticipantID, EventID, CategoryID, EnrollmentDate)
                OUTPUT INSERTED.EnrollmentID
                VALUES (@ParticipantID, @EventID, @CategoryID, @Date)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ParticipantID", SqlDbType.Int)
                .Value = enrollment.ParticipantID;
            cmd.Parameters.Add("@EventID", SqlDbType.Int)
                .Value = enrollment.EventID;
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int)
                .Value = enrollment.CategoryID;
            cmd.Parameters.Add("@Date", SqlDbType.DateTime2)
                .Value = DateTime.UtcNow;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql =
                "DELETE FROM Enrollments WHERE EnrollmentID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
