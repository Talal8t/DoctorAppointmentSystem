using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class AppointmentRep
    {
        private readonly string _connection;

        public AppointmentRep(string connection)
        {
            _connection = connection;
        }

        // ---------------- BOOK APPOINTMENT ----------------
        public async Task<string> BookAppointment(
            int doctorId,
            int patientId,
            int slotId,
            int tokenNumber,
            DateOnly appointmentDate,
            DateTime createdAt)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"INSERT INTO Appointments 
                            (DoctorId, PatientId, SlotId, TokenNumber, Status, AppointmentDate, CreatedAt)
                            VALUES 
                            (@DoctorId, @PatientId, @SlotId, @TokenNumber, 'Booked', @AppointmentDate, @CreatedAt)";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@PatientId", patientId);
            cmd.Parameters.AddWithValue("@SlotId", slotId);
            cmd.Parameters.AddWithValue("@TokenNumber", tokenNumber);
            cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@CreatedAt", createdAt);

            await conn.OpenAsync();

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0 ? "Appointment booked successfully." : "Failed to book appointment.";
        }

        // ---------------- CANCEL ----------------
        public async Task<int> CancelAppointment(int appointmentId)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = "UPDATE Appointments SET Status = 'Cancelled' WHERE AppointmentId = @AppointmentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ---------------- COMPLETE ----------------
        public async Task<int> CompleteAppointment(int appointmentId)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = "UPDATE Appointments SET Status = 'Completed' WHERE AppointmentId = @AppointmentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ---------------- GET BY PATIENT ----------------
        public async Task<List<Appointment>> GetAppointmentsByPatient(int patientId)
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = "SELECT * FROM Appointments WHERE PatientId = @PatientId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- GET BY DOCTOR ----------------
        public async Task<List<Appointment>> GetAppointmentsByDoctor(int doctorId)
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = "SELECT * FROM Appointments WHERE DoctorId = @DoctorId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- GET BY ID ----------------
        public async Task<Appointment?> GetAppointmentById(int appointmentId)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = "SELECT * FROM Appointments WHERE AppointmentId = @AppointmentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return Map(reader);

            return null;
        }

        // ---------------- GET ALL ----------------
        public async Task<List<Appointment>> GetAllAppointments()
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = "SELECT * FROM Appointments";

            using SqlCommand cmd = new SqlCommand(query, conn);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- UPDATE ----------------
        public async Task<int> UpdateAppointment(
            int appointmentId,
            int doctorId,
            int patientId,
            DateOnly appointmentDate,
            int slotId,
            int tokenNumber,
            string status)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"UPDATE Appointments 
                            SET DoctorId = @DoctorId,
                                PatientId = @PatientId,
                                SlotId = @SlotId,
                                TokenNumber = @TokenNumber,
                                Status = @Status,
                                AppointmentDate = @AppointmentDate
                            WHERE AppointmentId = @AppointmentId";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@PatientId", patientId);
            cmd.Parameters.AddWithValue("@SlotId", slotId);
            cmd.Parameters.AddWithValue("@TokenNumber", tokenNumber);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ---------------- GET UPCOMING ----------------
        public async Task<List<Appointment>> GetUpcomingAppointments()
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"SELECT * FROM Appointments 
                             WHERE AppointmentDate >= @Today AND Status = 'Booked'";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Today", DateTime.Now.Date);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- GET PAST ----------------
        public async Task<List<Appointment>> GetPastAppointments()
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"SELECT * FROM Appointments 
                             WHERE AppointmentDate < @Today AND Status = 'Completed'";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Today", DateTime.Now.Date);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- GET BY DATE ----------------
        public async Task<List<Appointment>> GetAppointmentsByDate(DateOnly date)
        {
            List<Appointment> appointments = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = "SELECT * FROM Appointments WHERE AppointmentDate = @Date";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(Map(reader));
            }

            return appointments;
        }

        // ---------------- MAPPER (IMPORTANT CLEAN FIX) ----------------
        private Appointment Map(SqlDataReader reader)
        {
            return new Appointment
            {
                AppointmentId = (int)reader["AppointmentId"],
                DoctorId = (int)reader["DoctorId"],
                PatientId = (int)reader["PatientId"],
                SlotId = (int)reader["SlotId"],
                TokenNumber = (int)reader["TokenNumber"],
                Status = reader["Status"].ToString(),
                AppointmentDate = DateOnly.FromDateTime((DateTime)reader["AppointmentDate"]),
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
        public async Task<int?> GetMaxTokenNumber(int doctorId, DateOnly date)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
        SELECT ISNULL(MAX(TokenNumber), 0)
FROM Appointments WITH (UPDLOCK, HOLDLOCK)
WHERE DoctorId = @DoctorId
AND AppointmentDate = @Date";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();

            object result = await cmd.ExecuteScalarAsync();

            if (result == DBNull.Value || result == null)
                return 0;

            return Convert.ToInt32(result);
        }
        public async Task<int?> GetLastTokenNumber(int doctorId, DateOnly date)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
        SELECT TOP 1 TokenNumber
        FROM Appointments
        WHERE DoctorId = @DoctorId
        AND AppointmentDate = @Date
        ORDER BY CreatedAt DESC";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();

            object result = await cmd.ExecuteScalarAsync();

            if (result == null)
                return 0;

            return Convert.ToInt32(result);
        }
        public async Task ResetTokensByDate(DateOnly date)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
        UPDATE Appointments
        SET TokenNumber = 0
        WHERE AppointmentDate = @Date";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<bool> IsSlotAlreadyBooked(int doctorId, int slotId, DateOnly date)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
        SELECT COUNT(1)
        FROM Appointments
        WHERE DoctorId = @DoctorId
          AND SlotId = @SlotId
          AND AppointmentDate = @Date
          AND Status = 'Booked'";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@SlotId", slotId);
            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
            await conn.OpenAsync();

            int count = (int)await cmd.ExecuteScalarAsync();

            return count > 0;
        }

        public bool HasBookedAppointmentBetweenDoctorAndPatient(int doctorId, int patientId)
        {
            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
                SELECT COUNT(1)
                FROM Appointments
                WHERE DoctorId = @DoctorId
                  AND PatientId = @PatientId
                  AND Status = 'Booked'";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public List<int> GetPatientIdsForDoctorOnDate(int doctorId, DateOnly date)
        {
            List<int> patientIds = new();

            using SqlConnection conn = new SqlConnection(_connection);

            string query = @"
                SELECT DISTINCT PatientId
                FROM Appointments
                WHERE DoctorId = @DoctorId
                  AND AppointmentDate = @Date
                  AND Status = 'Booked'";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                patientIds.Add((int)reader["PatientId"]);

            return patientIds;
        }
    }
}
