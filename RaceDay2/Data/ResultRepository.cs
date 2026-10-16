
using Microsoft.Data.SqlClient;
using RaceDay2.Models;
using System.Data;

namespace RaceDay2.Data
{
    public class ResultRepository
    {
        private readonly DbConnectionFactory _factory;

        public ResultRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Result?> GetByEnrollmentAsync(int enrollmentId)
        {
            const string sql = @"
                SELECT ResultID, EnrollmentID, FinishTime, FinishingPosition
                FROM Results
                WHERE EnrollmentID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = enrollmentId;

            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapResult(reader) : null;
        }

        public async Task<List<Result>> GetByEventAsync(int eventId)
        {
            const string sql = @"
                SELECT r.ResultID, r.EnrollmentID,
                       r.FinishTime, r.FinishingPosition
                FROM Results r
                INNER JOIN Enrollments e
                    ON r.EnrollmentID = e.EnrollmentID
                WHERE e.EventID = @ID
                ORDER BY r.FinishingPosition";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = eventId;

            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<Result>();

            while (await reader.ReadAsync())
                results.Add(MapResult(reader));

            return results;
        }

        private static Result MapResult(SqlDataReader reader)
        {
            return new Result
            {
                ResultID = reader.GetInt32(reader.GetOrdinal("ResultID")),
                EnrollmentID = reader.GetInt32(reader.GetOrdinal("EnrollmentID")),
                FinishTime = reader.GetTimeSpan(reader.GetOrdinal("FinishTime")),
                FinishingPosition = reader.GetInt32(
                    reader.GetOrdinal("FinishingPosition"))
            };
        }

        public async Task<int> CreateAsync(Result result)
        {
            const string sql = @"
                INSERT INTO Results
                    (EnrollmentID, FinishTime, FinishingPosition)
                OUTPUT INSERTED.ResultID
                VALUES (@EnrollmentID, @FinishTime, @Position)";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@EnrollmentID", SqlDbType.Int)
                .Value = result.EnrollmentID;
            cmd.Parameters.Add("@FinishTime", SqlDbType.Time)
                .Value = result.FinishTime;
            cmd.Parameters.Add("@Position", SqlDbType.Int)
                .Value = result.FinishingPosition;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(Result result)
        {
            const string sql = @"
                UPDATE Results
                SET FinishTime = @FinishTime,
                    FinishingPosition = @Position
                WHERE ResultID = @ID";

            using var con = _factory.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = result.ResultID;
            cmd.Parameters.Add("@FinishTime", SqlDbType.Time)
                .Value = result.FinishTime;
            cmd.Parameters.Add("@Position", SqlDbType.Int)
                .Value = result.FinishingPosition;

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
