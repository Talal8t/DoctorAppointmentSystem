using DAMS.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAMS.Repositories;

namespace DAMS.Services
{
    public class MiddleMan
    {
        #region ================= DEPENDENCIES =================

        private readonly AppointmentService _appointmentService;
        private readonly TokenGenerationService _tokenService;
        private readonly QueueService _queueService;
        private readonly DoctorSlotServices _slotService;
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;
        private readonly UserService _userService;
        private readonly DepServices _depService;
        private readonly AuthService _authService;
        private readonly NotificationService _notificationService;
        private readonly OPDRepository _opdRepo;

        public MiddleMan(
            AppointmentService appointmentService,
            TokenGenerationService tokenService,
            QueueService queueService,
            DoctorSlotServices slotService,
            DoctorService doctorService,
            PatientService patientService,
            UserService userService,
            DepServices depService,
            AuthService authService,
            NotificationService notificationService,
            OPDRepository opdRepo)
        {
            _appointmentService = appointmentService;
            _tokenService = tokenService;
            _queueService = queueService;
            _slotService = slotService;
            _doctorService = doctorService;
            _patientService = patientService;
            _userService = userService;
            _depService = depService;
            _authService = authService;
            _notificationService = notificationService;
            _opdRepo = opdRepo;
        }

        #endregion

        // =========================================================
        // ================= APPOINTMENT FLOW ======================
        // =========================================================

        #region Appointment Flow

        public async Task<Appointment> BookAppointmentFlow(Appointment appointment)
        {
            await _slotService.ValidateSlotForBooking(
                appointment.DoctorId,
                appointment.SlotId,
                appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));

            int token = await _tokenService.GenerateNextToken(
                appointment.DoctorId,
                appointment.AppointmentDate);

            appointment.TokenNumber = token;
            appointment.Status = "Booked";
            appointment.CreatedAt = DateTime.Now;

            await _appointmentService.BookAppointment(appointment);
            await _slotService.BookSlot(appointment.SlotId);

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (appointment.AppointmentDate == today)
                _queueService.EnqueueAppointment(appointment);

            await _notificationService.NotifyDoctorAppointmentBooked(
                appointment.DoctorId,
                appointment.PatientId);

            return appointment;
        }

        public async Task CancelAppointmentFlow(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Cancelled";

            await _appointmentService.UpdateAppointment(appointment);
            await _slotService.ReleaseSlot(appointment.SlotId);
            await _notificationService.SendNotification(appointment.DoctorId,"Doctor", $"Appointment {appointment.AppointmentId} cancelled by patient {appointment.PatientId}");
            _queueService.Remove(appointment.DoctorId, appointmentId);
        }

        public async Task CompleteAppointmentFlow(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Completed";

            await _appointmentService.UpdateAppointment(appointment);
            await _notificationService.NotifyAppointmentCompleted(appointment.PatientId);
        }

        public async Task MarkMissedFlow(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Missed";

            await _appointmentService.UpdateAppointment(appointment);
            await _notificationService.NotifyAppointmentMissed(appointment.PatientId);
        }

        public async Task MarkInProgressFlow(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "InProgress";

            await _appointmentService.UpdateAppointment(appointment);
        }

        public async Task<Appointment> RescheduleFlow(int appointmentId, DateOnly newDate)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found");

            if (appointment.Status != "Cancelled")
                throw new Exception("Only cancelled appointments can be rescheduled");

            await _slotService.ValidateSlotForBooking(
                appointment.DoctorId,
                appointment.SlotId,
                newDate.ToDateTime(TimeOnly.MinValue));

            bool alreadyBooked = await _appointmentService.IsSlotAlreadyBooked(
                appointment.DoctorId,
                appointment.SlotId,
                newDate);

            if (alreadyBooked)
                throw new Exception("Slot already booked by another patient");

            appointment.AppointmentDate = newDate;
            appointment.Status = "Booked";

            int token = await _tokenService.GenerateNextToken(
                appointment.DoctorId,
                newDate);

            appointment.TokenNumber = token;

            await _appointmentService.UpdateAppointment(appointment);
            await _slotService.BookSlot(appointment.SlotId);

            _queueService.EnqueueAppointment(appointment);

            return appointment;
        }

        //public async Task StartOPDQueueFlow(int doctorId)
        //{
        //    var date = DateOnly.FromDateTime(DateTime.Today);

        //    var appointments = await _appointmentService.GetAppointmentsByDoctorId(doctorId);

        //    var todayAppointments = appointments
        //        .Where(a => a.AppointmentDate == date &&
        //                    (a.Status == "Booked" || a.Status == "InProgress"))
        //        .ToList();

        //    _queueService.LoadQueue(doctorId, todayAppointments);
        //}

        //public void StopOPDQueueFlow(int doctorId)
        //{
        //    _queueService.ClearQueue(doctorId);
        //}

        //public async Task<List<Appointment>> LoadDoctorQueueFlow(int doctorId)
        //{
        //    var apps = await _appointmentService.GetAppointmentsByDoctorId(doctorId);
        //    _queueService.LoadQueue(doctorId, apps);
        //    return _queueService.GetCurrentQueue(doctorId);
        //}

        public async Task<List<Appointment>> GetPatientAppointmentsFlow(int patientId)
        {
            return await _appointmentService.GetAppointmentsByPatientId(patientId);
        }

        public async Task<bool> ChkPatientAppointmentsExist(int patientId)
        {
            var apps = await _appointmentService.GetAppointmentsByPatientId(patientId);
            return apps != null && apps.Count > 0;
        }

        public int GetPatientIdByUserId(int userId)
        {
            var patient = _patientService.GetPatientByUserId(userId);
            return patient != null ? patient.PatientId : 0;
        }

        public async Task<Appointment> GetAppointmentByIdFlow(int appointmentId)
        {
            return await _appointmentService.GetAppointmentById(appointmentId);
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdFlow(int doctorId)
        {
            return await _appointmentService.GetAppointmentsByDoctorId(doctorId);
        }

        #endregion

        // =========================================================
        // ================= PRIORITY ==============================
        // =========================================================

        #region Priority

        public async Task EmergencyAppointmentFlow(Appointment appointment)
        {
            int token = await _tokenService.GenerateNextToken(
                appointment.DoctorId,
                appointment.AppointmentDate);

            appointment.TokenNumber = token;
            appointment.Status = "Booked";
            appointment.Priority = PriorityLevel.Emergency;
            appointment.IsEmergency = true;

            await _slotService.ValidateSlotForBooking(
                appointment.DoctorId,
                appointment.SlotId,
                appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));

            await _appointmentService.BookAppointment(appointment);
            await _slotService.BookSlot(appointment.SlotId);

            _queueService.EnqueueAppointment(appointment);
        }

        public async Task PromoteToEmergency(int appointmentId, DateOnly date)
        {
            var appointment = await _appointmentService.GetAppointmentById(appointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Priority = PriorityLevel.Emergency;

            await _appointmentService.UpdateAppointment(appointment);
            _queueService.Reinsert(appointment);
        }

        #endregion

        // =========================================================
        // ================= SLOT & VALIDATION =====================
        // =========================================================

        #region Slot & Validation

        public async Task<bool> ValidateSlotFlow(int doctorId, int slotId, DateTime date)
        {
            return await _slotService.IsSlotAvailable(doctorId, slotId, date);
        }

        public async Task<string> CreateDoctorSlotFlow(DoctorSlot slot)
        {
            return await _slotService.AddDoctorSlotAsync(slot);
        }

        public async Task<bool> ValidateTokenFlow(int doctorId, int token, DateOnly date)
        {
            return await _tokenService.IsTokenValid(doctorId, token, date);
        }

        public async Task CloseDoctorSlotFlow(int doctorId, int slotId, DateTime date)
        {
            if (!await _doctorService.DoctorExists(doctorId))
                throw new Exception("Doctor not found");

            var slot = await _slotService.GetDoctorSlotByIdAsync(slotId);

            if (slot == null)
                throw new Exception("Slot not found");

            if (slot.DoctorId != doctorId)
                throw new Exception("Slot does not belong to doctor");

            if (slot.SlotDate.Date < DateTime.Today)
                throw new Exception("Cannot close past slot");

            await _slotService.CloseSlot(slotId);

            var appointments = await _appointmentService.GetAppointmentsByDoctorId(doctorId);

            foreach (var app in appointments.Where(a => a.SlotId == slotId && a.Status == "Booked"))
            {
                app.Status = "Cancelled";
                await _appointmentService.UpdateAppointment(app);

                _queueService.Remove(app.DoctorId, app.AppointmentId);
                await _slotService.ReleaseSlot(app.SlotId);
            }
        }

        public async Task GenerateDoctorSlots(int doctorId, DateTime date, TimeSpan start, TimeSpan end)
        {
            List<DoctorSlot> slots = new();

            var current = start;

            while (current < end)
            {
                slots.Add(new DoctorSlot
                {
                    DoctorId = doctorId,
                    SlotDate = date.Date,
                    SlotTime = current,
                    Status = "Available"
                });

                current = current.Add(TimeSpan.FromMinutes(15));
            }

            foreach (var slot in slots)
                await _slotService.AddDoctorSlotAsync(slot);
        }

        #endregion

        // =========================================================
        // ================= USER / AUTH ===========================
        // =========================================================

        #region User & Auth

        public User LoginFlow(string email, string password)
            => _authService.Login(email, password);

        public void RegisterFlow(User user)
            => _authService.Register(user);

        public void UpdateUserFlow(User user, User currentUser)
            => _userService.UpdateUser(user, currentUser);

        public void DeleteUserFlow(int userId, User currentUser)
            => _userService.DeleteUser(userId, currentUser);

        public bool RoleCheckFlow(User user, string role)
            => _authService.IsInRole(user, role);

        public bool PatientExistsByUserId(int userId)
            => _patientService.ChkPatientByUserId(userId);

        public bool DoctorExistsByUserId(int userId)
            => _doctorService.GetDoctorByUserId(userId) != null;

        public bool AdminExistsByUserId(int userId)
            => _userService.GetUserById(userId);

        #endregion

        // =========================================================
        // ================= PATIENT ===============================
        // =========================================================

        #region Patient

        public void RegisterPatientFlow(Patient patient)
            => _patientService.AddPatient(patient);

        public Patient GetPatientFlow(int id)
            => _patientService.GetPatientById(id);

        public void UpdatePatientFlow(Patient patient)
            => _patientService.UpdatePatient(patient);

        public async Task<List<DoctorSlot>> GetAvailableSlotsFlow(int doctorId, DateTime date)
            => await _slotService.GetAvailableSlotsByDateandDoctor(doctorId, date);

        #endregion

        // =========================================================
        // ================= DOCTOR ================================
        // =========================================================

        #region Doctor

        public void AddDoctorFlow(Doctor doctor)
            => _doctorService.AddDoctor(doctor);

        public void UpdateDoctorFlow(Doctor doctor)
            => _doctorService.UpdateDoctor(doctor);

        public void DeleteDoctorFlow(int id)
            => _doctorService.DeleteDoctor(id);

        public async Task<int> GetDoctorIdByUserId(int userId)
        {
            var doctor = await _doctorService.GetDoctorByUserIdFlow(userId);
            return doctor != null ? doctor.DoctorId : 0;
        }

        public Doctor GetDoctorFlow(int id)
            => _doctorService.GetDoctorById(id);

        public List<Doctor> GetDoctorsByDepartmentFlow(int departmentId)
            => _doctorService.GetDoctorsByDepartment(departmentId);

        #endregion

        // =========================================================
        // ================= SYSTEM RECOVERY =======================
        // =========================================================

        #region System Recovery

        public async Task RecoverSystem(DateOnly date)
        {
            var appointments = await _appointmentService.GetAppointmentsByDate(date);

            foreach (var app in appointments)
            {
                if (app.Status == "Booked" || app.Status == "InProgress")
                {
                    _queueService.Remove(app.DoctorId, app.AppointmentId);
                    _queueService.EnqueueAppointment(app);
                }
            }
        }

        #endregion

        // =========================================================
        // ================= DEPARTMENT ============================
        // =========================================================

        #region Department

        public void AddDepartmentFlow(string name)
            => _depService.AddDepartment(name);

        public void UpdateDepartmentFlow(int id, string name)
            => _depService.UpdateDepartment(id, name);

        public void DeleteDepartmentFlow(int id)
            => _depService.DeleteDepartment(id);

        public List<Department> GetAllDepartmentsFlow()
            => _depService.GetAll();

        public List<Doctor> GetDoctorsByDepartment(int departmentId)
            => _doctorService.GetDoctorsByDepartment(departmentId);

        #endregion

        // =========================================================
        // ================= QUEUE CONTROL =========================
        // =========================================================

        #region Queue

        public async Task<string> ToggleSlotFlow(int slotId, User currentUser)
        {
            if (currentUser.Role != "Admin")
                throw new Exception("Only admin can toggle slot");

            return await _slotService.ToggleSlotStatus(slotId);
        }

        //public async Task LoadDoctorQueue(int doctorId)
        //{
        //    var apps = await _appointmentService.GetAppointmentsByDoctorId(doctorId);
        //    _queueService.LoadQueue(doctorId, apps);
        //}

        public async Task<QueueResult> CallNextPatient(int doctorId)
        {
            // 🔥 DB CHECK HERE (correct layer)
            if (!_opdRepo.IsOPDStarted(doctorId))
            {
                return new QueueResult
                {
                    Success = false,
                    Message = "OPD is not started"
                };
            }
            if (!_queueService._doctorQueueStatus.ContainsKey(doctorId))
            {
                bool isStarted = _opdRepo.IsOPDStarted(doctorId);
                _queueService._doctorQueueStatus[doctorId] = isStarted;
            }
            var appointment = _queueService.CallNextPatient(doctorId);

            if (appointment == null)
            {
                return new QueueResult
                {
                    Success = false,
                    Message = "No patient or queue inactive"
                };
            }
            

            appointment.Status = "InProgress";
            await _appointmentService.UpdateAppointment(appointment);

            return new QueueResult
            {
                Success = true,
                Data = appointment
            };
        }
        

        public async Task CancelAppointment(int appointmentId)
        {
            var app = await _appointmentService.GetAppointmentById(appointmentId);

            if (app == null)
                throw new Exception("Appointment not found");

            app.Status = "Cancelled";

            await _appointmentService.UpdateAppointment(app);
            _queueService.Remove(app.DoctorId, appointmentId);
        }

        public async Task CompleteAppointment(int appointmentId)
        {
            var app = await _appointmentService.GetAppointmentById(appointmentId);

            if (app == null)
                throw new Exception("Appointment not found");

            app.Status = "Completed";

            await _appointmentService.UpdateAppointment(app);
        }

        public async Task SyncDoctorQueue(int doctorId)
        {
            var apps = await _appointmentService.GetAppointmentsByDoctorId(doctorId);
            _queueService.LoadQueue(doctorId, apps);
        }

        public async Task<List<Appointment>> GetLiveQueue(int doctorId)
        {
            await EnsureQueueLoaded(doctorId);
            return _queueService.GetCurrentQueue(doctorId);
        }

        public async Task EnsureQueueLoaded(int doctorId)
        {
            var apps = await _appointmentService.GetAppointmentsByDoctorId(doctorId);

            var queue = apps
                .Where(a => a.DoctorId == doctorId &&
                            (a.Status == "Booked" || a.Status == "InProgress"))
                .OrderBy(a => a.Priority)
                .ThenBy(a => a.TokenNumber)
                .ToList();

            _queueService.LoadQueue(doctorId, queue);
        }

        #endregion
    }
}