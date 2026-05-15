using DAMS.Models;
using DAMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace DAMS.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PatientController : Controller
    {
        private readonly MiddleMan _middleMan;
        private readonly NotificationService _notificationService;

        public PatientController(MiddleMan middleMan, NotificationService notificationService)
        {
            _middleMan = middleMan;
            _notificationService = notificationService;
        }
        [HttpGet]
        public IActionResult CompletePatientProfile()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CompletePatientProfile(Patient model)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            model.UserId = userId;

            _middleMan.RegisterPatientFlow(model);

            return RedirectToAction("Dashboard");
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (!_middleMan.PatientExistsByUserId(userId.Value))
                return RedirectToAction("CompletePatientProfile");

            int patientId = _middleMan.GetPatientIdByUserId(userId.Value);

            var appointments = await _middleMan.GetPatientAppointmentsFlow(patientId);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var todayAppointments = appointments
                .Where(a => a.AppointmentDate == today && a.Status == "Booked")
                .ToList();

            ViewBag.TodayAppointments = todayAppointments;

            // ⭐ IMPORTANT: initialize queue for each doctor
            var doctorIds = todayAppointments
                .Select(x => x.DoctorId)
                .Distinct();

            foreach (var docId in doctorIds)
            {
                await _middleMan.EnsureQueueLoaded(docId);
            }

            return View(appointments);
        }

        // ================= VIEW APPOINTMENTS =================
        public async Task<IActionResult> MyAppointmentsAsync(string status = "All")
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            int patientId = _middleMan.GetPatientIdByUserId(userId.Value);

            var appointments = await _middleMan.GetPatientAppointmentsFlow(patientId);

            if (appointments == null || !appointments.Any())
            {
                ViewBag.Message = "No appointments found.";
                ViewBag.SelectedStatus = status;
                return View(new List<Appointment>());
            }

            // Load doctor info
            foreach (var appt in appointments)
            {
                appt.Doctor = _middleMan.GetDoctorFlow(appt.DoctorId);
            }

            // 🔥 FILTER LOGIC
            if (status != "All")
            {
                appointments = appointments
                    .Where(a => a.Status == status)
                    .ToList();
            }

            ViewBag.SelectedStatus = status;

            return View(appointments);
        }

        // ================= BOOK APPOINTMENT (GET) =================
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var departments = _middleMan.GetAllDepartmentsFlow(); 
            ViewBag.Departments = departments;

            return View();
        }

        // ================= BOOK APPOINTMENT (POST) =================
        [HttpPost]
        public async Task<IActionResult> BookAppointment(Appointment model)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                model.PatientId = _middleMan.GetPatientIdByUserId(userId.Value);

                
                if (model.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    ViewBag.Error = "Cannot book past date";

                    // 🔥 ADD THIS
                    ViewBag.Departments = _middleMan.GetAllDepartmentsFlow();

                    return View(model);
                }

                await _middleMan.BookAppointmentFlow(model);

                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                // 🔥 ADD THIS ALSO
                ViewBag.Departments = _middleMan.GetAllDepartmentsFlow();

                return View(model);
            }
        }

        public async Task<IActionResult> GetDoctorsByDepartment(int departmentId)
        {
            var doctors = _middleMan.GetDoctorsByDepartmentFlow(departmentId);
            return PartialView("_DoctorDropdown", doctors);
        }

        public async Task<IActionResult> GetSlots(int doctorId, DateTime date)
        {
            var slots = await _middleMan.GetAvailableSlotsFlow(doctorId, date);

            if (slots == null || !slots.Any())
                return Content("<p style='color:red'>No slots available</p>");

            return PartialView("_SlotTable", slots);
        }

        // ================= CANCEL APPOINTMENT =================
        public async Task<IActionResult> CancelAppointment(int id)
        {
            await _middleMan.CancelAppointmentFlow(id);
            return RedirectToAction("MyAppointments");
        }

        // ================= RESCHEDULE (GET) =================
        public async Task<IActionResult> RescheduleAsync(int id)
        {
            var appointment = await _middleMan.GetAppointmentByIdFlow(id);

            if (appointment == null)
                return RedirectToAction("MyAppointments");

            return View(appointment);
        }


        // ================= RESCHEDULE (POST) =================
        [HttpPost]
        public async Task<IActionResult> Reschedule(Appointment model)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                // ❌ prevent past date booking
                if (model.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    ViewBag.Error = "Cannot reschedule to past date";
                    return View(model);
                }

                await _middleMan.RescheduleFlow(model.AppointmentId, model.AppointmentDate);

                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }
        // ================= VIEW PROFILE =================
        [HttpGet]
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            int patientId = _middleMan.GetPatientIdByUserId(userId.Value);

            var patient = _middleMan.GetPatientFlow(patientId);

            return View(patient);
        }
        [HttpPost]
        public IActionResult Profile(Patient model)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                    return RedirectToAction("Login", "Auth");

                int patientId = _middleMan.GetPatientIdByUserId(userId.Value);

                // 👇 STEP 1: Get existing data from DB
                var existing = _middleMan.GetPatientFlow(patientId);

                // 👇 STEP 2: Merge (VERY IMPORTANT)
                existing.PatientName = string.IsNullOrWhiteSpace(model.PatientName)
                    ? existing.PatientName : model.PatientName;

                existing.City = string.IsNullOrWhiteSpace(model.City)
                    ? existing.City : model.City;

                existing.Phone = string.IsNullOrWhiteSpace(model.Phone)
                    ? existing.Phone : model.Phone;

                existing.Gender = string.IsNullOrWhiteSpace(model.Gender)
                    ? existing.Gender : model.Gender;

                existing.DateOfBirth = model.DateOfBirth == default
                    ? existing.DateOfBirth : model.DateOfBirth;

                // 👇 STEP 3: Update
                _middleMan.UpdatePatientFlow(existing);

                TempData["Success"] = "Profile updated successfully";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }
        [HttpGet("Patient/LiveQueue")]
        public async Task<IActionResult> LiveQueue(int doctorId)
        {
            var queue = await _middleMan.GetLiveQueue(doctorId);
            return PartialView("_LiveQueue", queue);
        }
        public async Task<IActionResult> GetNotifications()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var list = await _notificationService.GetUnread(userId, "Patient");

            return PartialView("_NotificationPartial", list);
        }
    }
}