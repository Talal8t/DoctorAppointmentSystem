using DAMS.Models;

namespace DAMS.ViewModels
{
    public class DoctorDashboardVM
    {
        public Appointment? Current { get; set; }
        public Appointment? Next { get; set; }
        public Appointment? Last { get; set; }

        public List<Appointment> TodayAppointments { get; set; } = new();

        public int TotalPatients => TodayAppointments.Count;
        public int Completed => TodayAppointments.Count(x => x.Status == "Completed");
        public int Pending => TodayAppointments.Count(x => x.Status == "Booked");
        public int Missed => TodayAppointments.Count(x => x.Status == "Missed");
    }
}
