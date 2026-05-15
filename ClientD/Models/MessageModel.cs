using System;
using System.Collections.Generic;
using System.Text;

namespace ClientD.Models
{
    public class MessageModel
    {
        public int SenderId { get; set; }

        public int? ReceiverId { get; set; }

        public List<int>? ReceiverIds { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
