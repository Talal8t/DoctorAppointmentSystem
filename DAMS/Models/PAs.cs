namespace DAMS.Models
{
    public class PAs
    {
        public int PAId { get; set; }

        public int UserId { get; set; }

        public int DoctorId { get; set; }

        // Navigation
        public User User { get; set; }
        public Doctor Doctor { get; set; }
    }
}
