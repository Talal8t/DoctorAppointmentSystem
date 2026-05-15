using DAMS.Models;
using System.Data;
using System.Data.SqlClient;

namespace DAMS.Repositories
{
    public class OPDRepository
    {
        private readonly string _connectionString;

        public OPDRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ================= GET STATUS =================
        public OPDStatus? GetByDoctorId(int doctorId)
        {
            using var con = new SqlConnection(_connectionString);
            string query = "SELECT TOP 1 * FROM OPDStatus WHERE DoctorId = @DoctorId ORDER BY Id DESC";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            con.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new OPDStatus
            {
                Id = (int)reader["Id"],
                DoctorId = (int)reader["DoctorId"],
                IsOPDStarted = (bool)reader["IsOPDStarted"],
                StartedAt = reader["StartedAt"] as DateTime?,
                StoppedAt = reader["StoppedAt"] as DateTime?,
                UpdatedAt = (DateTime)reader["UpdatedAt"]
            };
        }

        // ================= START OPD =================
        public void StartOPD(int doctorId)
        {
            using var con = new SqlConnection(_connectionString);
            string query = @"
                INSERT INTO OPDStatus (DoctorId, IsOPDStarted, StartedAt, UpdatedAt,OPDDate)
                VALUES (@DoctorId, 1, GETDATE(), GETDATE(), CAST(GETDATE() AS DATE))";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            con.Open();
            cmd.ExecuteNonQuery();
        }


        // ================= STOP OPD =================
        public void StopOPD(int doctorId)
        {
            using var con = new SqlConnection(_connectionString);
            string query = @"
                UPDATE OPDStatus
                SET IsOPDStarted = 0,
                    StoppedAt = GETDATE(),
                    UpdatedAt = GETDATE()
                WHERE DoctorId = @DoctorId AND OPDDate = CAST(GETDATE() AS DATE) AND IsOPDStarted = 1";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= CHECK OPD =================
        public bool IsOPDStarted(int doctorId)
        {
            using var con = new SqlConnection(_connectionString);

            string query = @"
        SELECT TOP 1 IsOPDStarted
        FROM OPDStatus
        WHERE DoctorId = @DoctorId
        AND OPDDate = CAST(GETDATE() AS DATE)
        ORDER BY Id DESC";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;

            con.Open();

            var result = cmd.ExecuteScalar();

            
            if (result == null || result == DBNull.Value)
                return false;

            return Convert.ToBoolean(result);
        }
    }
}