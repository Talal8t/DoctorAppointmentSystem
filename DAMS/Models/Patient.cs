namespace DAMS.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        public int UserId { get; set; }

        public string City { get; set; }

        public string Phone { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        // Navigation
        public User User { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}
