using DAMS.Models;

namespace DAMS.ViewModels
{
    public class AppointmentFilterVM
    {
        public List<Appointment> Appointments { get; set; }

        public string Status { get; set; }
        public DateTime? Date { get; set; }
    }
}
