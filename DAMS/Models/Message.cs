namespace DAMS.Models
{
    public class Message
    {

        public int MessageId { get; set; }

        public string ChatId { get; set; }

        public int SenderId { get; set; }

        public int? ReceiverId { get; set; } // nullable for group

        public string Content { get; set; }

        public string MessageType { get; set; }
        // Private, Group, Broadcast

        public string Status { get; set; }
        // Sent, Delivered, Read

        public DateTime CreatedAt { get; set; }
        // Navigation
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
