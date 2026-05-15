using DAMS.Models;
using DAMS.Services;
using DAMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DAMS.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DoctorController : Controller
    {
        private readonly MiddleMan _middleMan;
        private readonly NotificationService _notificationService;
        private readonly DoctorRequestService _requestService;
        private readonly AdminControlService _adminControlService;

        public DoctorController(MiddleMan middleMan, NotificationService notificationService, DoctorRequestService requestService, AdminControlService adminControlService  )
        {
            _middleMan = middleMan;
            _notificationService = notificationService;
            _requestService = requestService;
            _adminControlService = adminControlService;
        }
        [HttpPost]
        public async Task<IActionResult> RequestStartOPD()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var doctor = _middleMan.GetDoctorFlow(userId);

            
            bool isRunning = _adminControlService.IsOPDRunning(doctor.DoctorId);

            if (isRunning)
            {
                TempData["Error"] = "OPD is already started.";
                return RedirectToAction("Dashboard");
            }

            await _requestService.SendRequest(doctor.DoctorId, "StartOPD");

            TempData["Success"] = "Start OPD request sent.";
            return RedirectToAction("Dashboard");
        }
        [HttpPost]
        public async Task<IActionResult> RequestStopOPD()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var doctor = _middleMan.GetDoctorFlow(userId);

            
            bool isRunning =  _adminControlService.IsOPDRunning(doctor.DoctorId);

            if (!isRunning)
            {
                TempData["Error"] = "OPD is not running. Cannot stop.";
                return RedirectToAction("Dashboard");
            }

            await _requestService.SendRequest(doctor.DoctorId, "StopOPD");

            TempData["Success"] = "Stop OPD request sent.";
            return RedirectToAction("Dashboard");
        }
        // ================= DASHBOARD =================

        public async Task<IActionResult> Dashboard(string filter = "today")
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var doctor = _middleMan.GetDoctorFlow(userId.Value);
            var queue = await _middleMan.GetLiveQueue(doctor.DoctorId);

            // Filters
            if (filter == "completed")
                queue = queue.Where(x => x.Status == "Completed").ToList();

            if (filter == "booked")
                queue = queue.Where(x => x.Status == "Booked").ToList();

            var model = new DoctorDashboardVM
            {
                TodayAppointments = queue,
                Current = queue.FirstOrDefault(x => x.Status == "InProgress"),
                Next = queue.FirstOrDefault(x => x.Status == "Booked"),
                Last = queue.LastOrDefault(x => x.Status == "Completed")
            };

            return View(model);
        }

        // ================= QUEUE =================

        public async Task<IActionResult> LiveQueue()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var doctor = _middleMan.GetDoctorFlow(userId.Value);
            var queue = await _middleMan.GetLiveQueue(doctor.DoctorId);

            return PartialView("_DoctorQueuePartial", queue);
        }

        public async Task<IActionResult> RefreshQueue()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var doctor = _middleMan.GetDoctorFlow(userId.Value);

            var queue = await _middleMan.GetLiveQueue(doctor.DoctorId);

            return PartialView("_DoctorQueuePartial", queue);
        }
        public async Task<IActionResult> RefreshCards()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            var queue = await _middleMan.GetLiveQueue(doctor.DoctorId);

            var model = new DoctorDashboardVM
            {
                TodayAppointments = queue,
                Current = queue.FirstOrDefault(x => x.Status == "InProgress"),
                Next = queue.FirstOrDefault(x => x.Status == "Booked"),
                Last = queue.LastOrDefault(x => x.Status == "Completed")
            };

            return PartialView("_DoctorCardsPartial", model);
        }


        // ================= QUEUE ACTIONS =================

        [HttpPost]
        public async Task<IActionResult> CallNext()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            var next = await _middleMan.CallNextPatient(doctor.DoctorId);

            TempData["CalledPatient"] = next?.Data?.PatientId;
            TempData["Token"] = next?.Data?.TokenNumber;

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> MarkCompleted(int appointmentId)
        {
            await _middleMan.CompleteAppointment(appointmentId);
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> MarkMissed(int appointmentId)
        {
            await _middleMan.MarkMissedFlow(appointmentId);
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> MarkInProgress(int appointmentId)
        {
            await _middleMan.MarkInProgressFlow(appointmentId);
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> SkipPatient(int appointmentId)
        {
            var app = await _middleMan.GetAppointmentByIdFlow(appointmentId);

            if (app == null)
                return RedirectToAction("Dashboard");

            app.Status = "Skipped";
            await _middleMan.CompleteAppointmentFlow(appointmentId);

            return RedirectToAction("Dashboard");
        }

        // ================= OPD CONTROL =================

        

        [HttpPost]
        public async Task<IActionResult> ResetQueue()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            await _middleMan.SyncDoctorQueue(doctor.DoctorId);

            return RedirectToAction("Dashboard");
        }

        // ================= SLOT MANAGEMENT =================

        public IActionResult ManageSlots()
        {
            return View();
        }
        public async Task<IActionResult> TodaySlots()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            var slots = await _middleMan.GetAvailableSlotsFlow(
                doctor.DoctorId,
                DateTime.Today
            );

            return View(slots);
        }

        [HttpGet]
        public IActionResult CreateSlots()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSlots(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            await _middleMan.GenerateDoctorSlots(
                doctor.DoctorId,
                date,
                startTime,
                endTime
            );

            return RedirectToAction("TodaySlots");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSlot(int slotId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            await _middleMan.ToggleSlotFlow(slotId, doctor.User);

            return RedirectToAction("TodaySlots");
        }

        [HttpPost]
        public async Task<IActionResult> CloseSlot(int slotId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var doctor = _middleMan.GetDoctorFlow(userId);

            await _middleMan.CloseDoctorSlotFlow(doctor.DoctorId, slotId, DateTime.Now);

            return RedirectToAction("TodaySlots");
        }

        // ================= DETAILS =================

        public async Task<IActionResult> Appointments()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var doctor = _middleMan.GetDoctorFlow(userId.Value);

            var list = await _middleMan.GetAppointmentsByDoctorIdFlow(doctor.DoctorId);

            var model = new AppointmentFilterVM
            {
                Appointments = list
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Appointments(string status, DateTime? date)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var doctor = _middleMan.GetDoctorFlow(userId.Value);

            var list = await _middleMan.GetAppointmentsByDoctorIdFlow(doctor.DoctorId);

            // FILTER BY STATUS
            if (!string.IsNullOrEmpty(status))
            {
                list = list.Where(x => x.Status == status).ToList();
            }

            // FILTER BY DATE
            if (date.HasValue)
            {
                list = list.Where(x => x.AppointmentDate.ToDateTime(TimeOnly.MinValue).Date == date.Value.Date).ToList();
            }

            var model = new AppointmentFilterVM
            {
                Appointments = list,
                Status = status,
                Date = date
            };

            return View(model);
        }
        public async Task<IActionResult> GetNotifications()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var list = await _notificationService.GetUnread(userId, "Doctor");

            return PartialView("_NotificationPartial", list);
        }
    }
}