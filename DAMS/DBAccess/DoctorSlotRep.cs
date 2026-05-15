using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class DoctorSlotRep
    {
        private readonly string _connectionString;
        public DoctorSlotRep(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<int> AddDoctorSlot(DoctorSlot slot)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO DoctorSlots (DoctorId, SlotDate, SlotTime, Status) VALUES (@DoctorId, @SlotDate, @SlotTime, @Status)";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", slot.DoctorId);
                command.Parameters.AddWithValue("@SlotDate", slot.SlotDate);
                command.Parameters.AddWithValue("@SlotTime", slot.SlotTime);
                command.Parameters.AddWithValue("@Status", slot.Status);
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> UpdateDoctorSlot(DoctorSlot slot)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE DoctorSlots SET DoctorId = @DoctorId, SlotDate = @SlotDate, SlotTime = @SlotTime, Status = @Status WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", slot.DoctorId);
                command.Parameters.AddWithValue("@SlotDate", slot.SlotDate);
                command.Parameters.AddWithValue("@SlotTime", slot.SlotTime);
                command.Parameters.AddWithValue("@Status", slot.Status);
                command.Parameters.AddWithValue("@SlotId", slot.SlotId);
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> DeleteDoctorSlot(int slotId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM DoctorSlots WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotId", slotId);
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }
        public async Task<DoctorSlot> GetDoctorSlotById(int slotId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotId", slotId);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    };
                }
                return null;
            }
        }
        public async Task<List<DoctorSlot>> GetDoctorSlotsByDoctorId(int doctorId)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE DoctorId = @DoctorId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", doctorId);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetAvailableSlotsByDoctorId(int doctorId)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE DoctorId = @DoctorId AND Status = 'Available'";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", doctorId);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetAvailableSlotsByDate(DateTime date)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE SlotDate = @SlotDate AND Status = 'Available'";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotDate", date);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetAvailableSlotsByDoctorIdandDate(int doctorId, DateTime date)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT * 
        FROM DoctorSlots 
        WHERE DoctorId = @DoctorId 
        AND Status = 'Available'
        AND (
            SlotDate > @Today
            OR (SlotDate = @Today AND SlotTime > @CurrentTime)
        )
        ORDER BY SlotDate, SlotTime";

                using SqlCommand command = new(query, connection);

                command.Parameters.AddWithValue("@DoctorId", doctorId);

                // normalize date (important)
                command.Parameters.AddWithValue("@Today", DateTime.Today);

                // current time cutoff
                command.Parameters.AddWithValue("@CurrentTime", DateTime.Now.TimeOfDay);

                await connection.OpenAsync();

                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }

            return slots;
        }
        public async Task<List<DoctorSlot>> GetAllDoctorSlots()
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots";
                using SqlCommand command = new(query, connection);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
        public async Task MarkSlotAsBooked(int slotId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE DoctorSlots SET Status = 'Booked' WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotId", slotId);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task MarkSlotAsAvailable(int slotId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE DoctorSlots SET Status = 'Available' WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotId", slotId);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> CheckSlotAvailability(int slotId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT Status FROM DoctorSlots WHERE SlotId = @SlotId";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotId", slotId);
                await connection.OpenAsync();
                var status = (string)await command.ExecuteScalarAsync();
                return status == "Available";
            }
        }
        public async Task<List<DoctorSlot>> GetSlotsByDate(DateTime date)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE SlotDate = @SlotDate";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@SlotDate", date);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetSlotsByDoctorIdAndDate(int doctorId, DateTime date)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE DoctorId = @DoctorId AND SlotDate = @SlotDate";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", doctorId);
                command.Parameters.AddWithValue("@SlotDate", date);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        } 
        public async Task<List<DoctorSlot>> GetSlotsByDoctorIdAndStatus(int doctorId, string status)
        {
            List<DoctorSlot> slots = new List<DoctorSlot>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DoctorSlots WHERE DoctorId = @DoctorId AND Status = @Status";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@DoctorId", doctorId);
                command.Parameters.AddWithValue("@Status", status);
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    slots.Add(new DoctorSlot
                    {
                        SlotId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        SlotDate = reader.GetDateTime(2),
                        SlotTime = reader.GetTimeSpan(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return slots;
        }
    }
}
