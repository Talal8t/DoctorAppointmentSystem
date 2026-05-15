namespace DAMS.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; }
        public string UserType { get; internal set; }
    }
}
