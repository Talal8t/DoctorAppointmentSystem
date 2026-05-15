using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Models
{
    internal class ServerResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }
}
