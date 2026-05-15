namespace DAMS.ViewModels
{
    public class AppointmentReportVM
    {
        public DateOnly Date { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
