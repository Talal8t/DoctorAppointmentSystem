using DAMS.Models;

namespace DAMS.ViewModels
{
    public class ChatViewModel
    {
        public string ChatId { get; set; } = string.Empty;
        public string Title { get; set; } = "Chat";
        public string Mode { get; set; } = "Private";
        public int CurrentUserId { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? BroadcastTarget { get; set; }
        public List<Message> Messages { get; set; } = new();
    }

    public class SendMessageRequest
    {
        public string Mode { get; set; } = "Private";
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? BroadcastTarget { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
