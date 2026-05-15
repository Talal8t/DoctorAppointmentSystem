namespace DAMS.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        public int UserId { get; set; }

        public int DepartmentId { get; set; }

        public string Specialization { get; set; }
        public string DoctorName { get; set; }  

        // Navigation
        public User User { get; set; }
        public Department Department { get; set; }

        public List<DoctorSlot> Slots { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<PAs> PAs { get; set; }
    }
}
