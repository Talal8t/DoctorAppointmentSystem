using System.Data.SqlClient;
using DAMS.Models;
using DAMS.Services;

namespace DAMS.DBAccess
{
    public class MessageRep
    {
        private readonly string _connectionString;

        public MessageRep(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Save(Message msg)
        {
            using var con = new SqlConnection(_connectionString);

            string query = @"
                INSERT INTO Messages
                (ChatId, SenderId, ReceiverId, Content, MessageType, Status, CreatedAt)
                VALUES
                (@ChatId, @SenderId, @ReceiverId, @Content, @MessageType, @Status, @CreatedAt)";

            using var cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@ChatId", msg.ChatId);
            cmd.Parameters.AddWithValue("@SenderId", msg.SenderId);
            cmd.Parameters.AddWithValue("@ReceiverId", (object?)msg.ReceiverId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Content", msg.Content);
            cmd.Parameters.AddWithValue("@MessageType", msg.MessageType);
            cmd.Parameters.AddWithValue("@Status", msg.Status);
            cmd.Parameters.AddWithValue("@CreatedAt", msg.CreatedAt);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Message> GetByChatId(string chatId)
        {
            var list = new List<Message>();

            using var con = new SqlConnection(_connectionString);

            string query = @"
                SELECT MessageId, ChatId, SenderId, ReceiverId, Content, MessageType, Status, CreatedAt
                FROM Messages
                WHERE ChatId = @ChatId
                ORDER BY CreatedAt ASC";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChatId", chatId);

            con.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Message> GetChatHistory(string chatId, int currentUserId, bool isBroadcast)
        {
            if (!isBroadcast)
                return GetByChatId(chatId);

            var list = new List<Message>();

            using var con = new SqlConnection(_connectionString);

            string query = @"
                SELECT MessageId, ChatId, SenderId, ReceiverId, Content, MessageType, Status, CreatedAt
                FROM Messages
                WHERE ChatId = @ChatId
                  AND SenderId = @CurrentUserId
                  AND ReceiverId IS NULL
                ORDER BY CreatedAt ASC";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChatId", chatId);
            cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId);

            con.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Message> GetBroadcastMessagesForUser(int userId)
        {
            var list = new List<Message>();

            using var con = new SqlConnection(_connectionString);

            string query = @"
                SELECT MessageId, ChatId, SenderId, ReceiverId, Content, MessageType, Status, CreatedAt
                FROM Messages
                WHERE MessageType <> 'Private'
                  AND (ReceiverId = @UserId OR (SenderId = @UserId AND ReceiverId IS NULL))
                ORDER BY CreatedAt DESC";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public List<Message> GetPrivateChat(int user1, int user2)
        {
            var chatId = ChatIdGenerator.PrivateChat(user1, user2);
            return GetByChatId(chatId);
        }

        public void UpdateStatus(int messageId, string status)
        {
            using var con = new SqlConnection(_connectionString);

            string query = @"
                UPDATE Messages
                SET Status = @Status
                WHERE MessageId = @MessageId";

            using var cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@MessageId", messageId);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        private static Message Map(SqlDataReader reader)
        {
            return new Message
            {
                MessageId = (int)reader["MessageId"],
                ChatId = reader["ChatId"].ToString() ?? string.Empty,
                SenderId = (int)reader["SenderId"],
                ReceiverId = reader["ReceiverId"] == DBNull.Value ? null : Convert.ToInt32(reader["ReceiverId"]),
                Content = reader["Content"].ToString() ?? string.Empty,
                MessageType = reader["MessageType"].ToString() ?? string.Empty,
                Status = reader["Status"].ToString() ?? string.Empty,
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
    }
}
