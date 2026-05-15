namespace DAMS.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; } = string.Empty; // Patient / Doctor / PA / Admin

        public DateTime CreatedAt { get; set; }
    }
}
