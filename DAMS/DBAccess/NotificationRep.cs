using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class NotificationRep
    {
        private readonly string _connectionString;

        public NotificationRep(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ================= ADD =================
        public async Task AddNotification(int userId, string userType, string message)
        {
            using SqlConnection con = new(_connectionString);

            string query = @"
                INSERT INTO Notifications (UserId, UserType, Message, IsRead, CreatedAt)
                VALUES (@UserId, @UserType, @Message, 0, GETDATE())";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@UserType", userType);
            cmd.Parameters.AddWithValue("@Message", message);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ================= GET UNREAD =================
        public async Task<List<Notification>> GetUnreadNotifications(int userId, string userType)
        {
            List<Notification> list = new();

            using SqlConnection con = new(_connectionString);

            string query = @"
                SELECT NotificationId, UserId, UserType, Message, IsRead, CreatedAt
                FROM Notifications
                WHERE UserId = @UserId
                AND UserType = @UserType
                AND IsRead = 0
                ORDER BY CreatedAt DESC";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@UserType", userType);

            await con.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Notification
                {
                    NotificationId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    UserType = reader.GetString(2),
                    Message = reader.GetString(3),
                    IsRead = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }

            return list;
        }

        // ================= MARK READ =================
        public async Task MarkNotificationsRead(int userId, string userType)
        {
            using SqlConnection con = new(_connectionString);

            string query = @"
                UPDATE Notifications
                SET IsRead = 1
                WHERE UserId = @UserId
                AND UserType = @UserType
                AND IsRead = 0";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@UserType", userType);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ================= COUNT =================
        public async Task<int> GetUnreadCount(int userId, string userType)
        {
            using SqlConnection con = new(_connectionString);

            string query = @"
                SELECT COUNT(*)
                FROM Notifications
                WHERE UserId = @UserId
                AND UserType = @UserType
                AND IsRead = 0";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@UserType", userType);

            await con.OpenAsync();

            return (int)await cmd.ExecuteScalarAsync();
        }
    }
}