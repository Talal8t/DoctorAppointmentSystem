using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Server.Models
{
    internal class UserModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Role Role { get; set; }
    }
}
