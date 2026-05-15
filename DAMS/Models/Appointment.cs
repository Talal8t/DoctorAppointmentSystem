using DAMS.Models;
using DAMS.Services;

namespace DAMS.Models
{

    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public int SlotId { get; set; }

        public int TokenNumber { get; set; }

        public string Status { get; set; } // Booked / Completed / Cancelled
        public bool IsEmergency { get; set; } = false;
        public DateOnly AppointmentDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public PriorityLevel Priority { get; set; }
        // Navigation
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public DoctorSlot Slot { get; set; }
    }
}
