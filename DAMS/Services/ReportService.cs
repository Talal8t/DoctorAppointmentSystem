using DAMS.Models;
using DAMS.Repositories;
using DAMS.ViewModels;

namespace DAMS.Services
{
    public class ReportService
    {
        private readonly AppointmentService _appointmentService;
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;

        public ReportService(
            AppointmentService appointmentService,
            DoctorService doctorService,
            PatientService patientService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _patientService = patientService;
        }

        // ================= DOCTOR REPORT =================
        public async Task<List<DoctorReportVM>> GetDoctorReports()
        {
            var doctors = _doctorService.GetAllDoctors();
            var appointments = await _appointmentService.GetAllAppointments();

            var appointmentGroups = appointments
     .GroupBy(a => a.DoctorId)
     .ToDictionary(g => g.Key, g => g.ToList());

            var report = doctors.Select(d =>
            {
                var doctorAppointments = appointmentGroups.ContainsKey(d.DoctorId)
                    ? appointmentGroups[d.DoctorId]
                    : new List<Appointment>();

                return new DoctorReportVM
                {
                    DoctorName = d.DoctorName,
                    TotalPatients = doctorAppointments.Count.ToString(),
                    IsActive = doctorAppointments.Any(a => a.Status == "InProgress")
                };
            }).ToList();

            return report;
        }

        // ================= PATIENT REPORT =================
        public async Task<List<PatientReportVM>> GetPatientReports()
        {
            var patients = _patientService.GetAllPatients();
            var appointments = await _appointmentService.GetAllAppointments();

            var appointmentGroups = appointments
    .GroupBy(a => a.PatientId)
    .ToDictionary(g => g.Key, g => g.Count());

            var report = patients.Select(p => new PatientReportVM
            {
                PatientName = p.PatientName,
                TotalVisits = appointmentGroups.ContainsKey(p.PatientId)
                    ? appointmentGroups[p.PatientId]
                    : 0,
                Department = "N/A"
            }).ToList();

            return report;
        }

        // ================= APPOINTMENT REPORT =================
        public async Task<List<AppointmentReportVM>> GetAppointmentReports()
        {
            var appointments = await _appointmentService.GetAllAppointments();
            var doctors = _doctorService.GetAllDoctors();
            var patients = _patientService.GetAllPatients();

            var report = appointments.Select(a => new AppointmentReportVM
            {
                Date = a.AppointmentDate,
                Status = a.Status,
                DoctorName = doctors.FirstOrDefault(d => d.DoctorId == a.DoctorId)?.DoctorName,
                PatientName = patients.FirstOrDefault(p => p.PatientId == a.PatientId)?.PatientName
            }).ToList();

            return report;
        }
    }
}