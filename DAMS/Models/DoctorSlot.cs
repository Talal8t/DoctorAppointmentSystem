namespace DAMS.Models
{
    public class DoctorSlot
    {
        public int SlotId { get; set; }

        public int DoctorId { get; set; }

        public DateTime SlotDate { get; set; }

        public TimeSpan SlotTime { get; set; }

        public string Status { get; set; } // Available / Booked

        // Navigation
        public Doctor Doctor { get; set; }
    }
}
