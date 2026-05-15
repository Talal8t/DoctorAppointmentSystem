using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRep _appointmentRepository;
        private readonly PatientService _patientService;
        private readonly DoctorService _doctorService;
        public AppointmentService(AppointmentRep appointmentRepository, PatientService patientService, DoctorService doctorService)
        {
            _appointmentRepository = appointmentRepository;
            _patientService = patientService;
            _doctorService = doctorService;
        }
        public AppointmentService()
        {
        }
        public async Task<string> BookAppointment(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null.");
            }
            if (appointment.DoctorId <= 0)
            {
                throw new ArgumentException("Invalid doctor ID.", nameof(appointment.DoctorId));
            }
            if (appointment.PatientId <= 0)
            {
                throw new ArgumentException("Invalid patient ID.", nameof(appointment.PatientId));
            }
            if (appointment.SlotId <= 0)
            {
                throw new ArgumentException("Invalid slot ID.", nameof(appointment.SlotId));
            }
            if (appointment.TokenNumber <= 0)
            {
                throw new ArgumentException("Invalid token number.", nameof(appointment.TokenNumber));
            }
            if (appointment.AppointmentDate == default)
            {
                throw new ArgumentException("Invalid appointment date.", nameof(appointment.AppointmentDate));
            }
            if (appointment.CreatedAt == default)
            {
                throw new ArgumentException("Invalid creation date.", nameof(appointment.CreatedAt));
            }

            return await _appointmentRepository.BookAppointment(appointment.DoctorId, appointment.PatientId, appointment.SlotId, appointment.TokenNumber, appointment.AppointmentDate, appointment.CreatedAt);
        }
        public async Task<List<Appointment>> GetAppointmentsByPatientId(int patientId)
        {
            if (patientId <= 0)
                throw new ArgumentException("Invalid patient ID.");

            if (!_patientService.PatientExists(patientId))
                throw new Exception("Patient not found.");

            return await _appointmentRepository.GetAppointmentsByPatient(patientId);
        }
        public async Task<List<Appointment>> GetAppointmentsByDoctorId(int doctorId)
        {
            if (doctorId <= 0)
            {
                throw new ArgumentException("Invalid doctor ID.", nameof(doctorId));
            }
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new ArgumentException("Doctor not found.", nameof(doctorId));
            }
            return await _appointmentRepository.GetAppointmentsByDoctor(doctorId);
        }
        public async Task<string> CancelAppointment(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("Invalid appointment ID", nameof(appointmentId));

            int result = await _appointmentRepository.CancelAppointment(appointmentId);

            if (result == 0)
                throw new Exception("Appointment not found");

            return "Appointment cancelled successfully.";
        }
        public async Task<string> CompleteAppointment(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("Invalid appointment ID", nameof(appointmentId));

            int result = await _appointmentRepository.CompleteAppointment(appointmentId);

            if (result == 0)
                throw new Exception("Appointment not found");

            return "Appointment completed successfully.";
        }
        public async Task<Appointment> GetAppointmentById(int appointmentId)
        {
            if (appointmentId <= 0)
            {
                throw new ArgumentException("Invalid appointment ID.", nameof(appointmentId));
            }
            if (!await AppointmentExists(appointmentId))
            {
                throw new ArgumentException("Appointment not found.", nameof(appointmentId));
            }
            return await _appointmentRepository.GetAppointmentById(appointmentId);
        }

        public async Task<List<Appointment>> GetAppointmentsByDate(DateOnly date)
        {
            if (date == default)
            {
                throw new ArgumentException("Invalid date.", nameof(date));
            }
            return await _appointmentRepository.GetAppointmentsByDate(date);
        }
        public async Task<string> UpdateAppointment(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null.");
            if (appointment.AppointmentId <= 0)
                throw new ArgumentException("Invalid appointment ID.", nameof(appointment.AppointmentId));
            if (!await AppointmentExists(appointment.AppointmentId))
                throw new ArgumentException("Appointment not found.", nameof(appointment.AppointmentId));
            int result = await _appointmentRepository.UpdateAppointment(appointment.AppointmentId, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.SlotId, appointment.TokenNumber, appointment.Status);
            if (result == 0)
                throw new Exception("Failed to update appointment.");
            return "Appointment updated successfully.";
        }
        public async Task<bool> IsSlotAlreadyBooked(int doctorId, int slotId, DateOnly date)
        {
            return await _appointmentRepository.IsSlotAlreadyBooked(doctorId, slotId, date);
        }
        public async Task<bool> AppointmentExists(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("Invalid appointment ID.", nameof(appointmentId));

            var appointment = await _appointmentRepository.GetAppointmentById(appointmentId);
            return appointment != null;
        }
        public async Task<List<Appointment>> GetAllAppointments()
        {
            return await _appointmentRepository.GetAllAppointments();
        }
    }
}
//✔ BookAppointment()
//✔ CancelAppointment()
//✔ CompleteAppointment()
//✔ RescheduleAppointment()

//✔ CallNextPatient()
//✔ MarkMissed()

//✔ ValidateAppointment()
//✔ CheckSlotAvailability()

//✔ GetAppointmentsByDoctor()
//✔ GetAppointmentsByPatient()
//✔ GetQueueView()

//✔ SyncWithQueueService()
//✔ SyncWithTokenService()