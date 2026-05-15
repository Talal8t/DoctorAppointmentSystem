using DAMS.Models;
using System.Data.SqlClient;
namespace DAMS.DBAccess
{
    public class DoctorRequestRep
    {
        private readonly string _conn;

        public DoctorRequestRep(string conn)
        {
            _conn = conn;
        }

        public async Task AddRequest(DoctorRequest req)
        {
            using var con = new SqlConnection(_conn);

            string query = @"
        INSERT INTO DoctorRequests (DoctorId, RequestType, TargetDoctorId, Status, CreatedAt)
        VALUES (@DoctorId, @RequestType, @TargetDoctorId, 'Pending', GETDATE())";

            using var cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@DoctorId", req.DoctorId);
            cmd.Parameters.AddWithValue("@RequestType", req.RequestType);
            cmd.Parameters.AddWithValue("@TargetDoctorId", (object?)req.TargetDoctorId ?? DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<DoctorRequest>> GetPendingRequests()
        {
            var list = new List<DoctorRequest>();

            using var con = new SqlConnection(_conn);

            string query = "SELECT * FROM DoctorRequests WHERE Status='Pending'";

            using var cmd = new SqlCommand(query, con);

            await con.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new DoctorRequest
                {
                    RequestId = reader.GetInt32(0),
                    DoctorId = reader.GetInt32(1),
                    RequestType = reader.GetString(2),
                    Status = reader.GetString(4)
                });
            }

            return list;
        }
        public async Task<DoctorRequest?> GetById(int requestId)
        {
            using var con = new SqlConnection(_conn);

            string query = "SELECT * FROM DoctorRequests WHERE RequestId = @RequestId";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RequestId", requestId);

            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            return new DoctorRequest
            {
                RequestId = (int)reader["RequestId"],
                DoctorId = (int)reader["DoctorId"],
                RequestType = reader["RequestType"].ToString(),
                Status = reader["Status"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }

        public async Task UpdateStatus(int requestId, string status)
        {
            using var con = new SqlConnection(_conn);

            string query = "UPDATE DoctorRequests SET Status=@Status WHERE RequestId=@Id";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Id", requestId);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<List<DoctorRequest>> GetAll()
        {
            var list = new List<DoctorRequest>();
            using var con = new SqlConnection(_conn);
            string query = "SELECT * FROM DoctorRequests";
            using var cmd = new SqlCommand(query, con);
            await con.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DoctorRequest
                {
                    RequestId = reader.GetInt32(0),
                    DoctorId = reader.GetInt32(1),
                    RequestType = reader.GetString(2),
                    Status = reader.GetString(4)
                });
            }
            return list;
        }
    }
}
