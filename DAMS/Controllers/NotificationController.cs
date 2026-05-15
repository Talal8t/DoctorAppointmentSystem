using DAMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace DAMS.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _service;
        private readonly MiddleMan _middlewareMan;
        public NotificationController(NotificationService service, MiddleMan middlewareMan)
        {
            _service = service;
            _middlewareMan = middlewareMan;
        }

        // ================= GET USER INFO =================
        private async Task<string> GetUserType()
        {
            // adjust this based on your login system
            int ? userId = HttpContext.Session.GetInt32("UserId");
            int patientId = _middlewareMan.GetPatientIdByUserId(userId ?? 0);
            if(patientId > 0)
              return "Patient";
            int doctorId = await _middlewareMan.GetDoctorIdByUserId(userId ?? 0);
            if(doctorId > 0)
              return "Doctor";
            return "null";
        }

        private int? GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // ================= GET NOTIFICATIONS =================
        public async Task<IActionResult> GetNotifications()
        {
            int? userId = GetUserId();
            string userType = await GetUserType();

            if (userId == null)
                return Unauthorized();

            var list = await _service.GetUnread(userId.Value, userType);

            return PartialView("_NotificationPartial", list);
        }

        // ================= GET COUNT =================
        public async Task<IActionResult> GetCount()
        {
            int? userId = GetUserId();
            string userType = await GetUserType();

            if (userId == null)
                return Content("0");

            var count = await _service.GetUnreadCount(userId.Value, userType);

            return Content(count.ToString());
        }

        // ================= MARK AS READ =================
        [HttpPost]
        public async Task<IActionResult> MarkRead()
        {
            int? userId = GetUserId();
            string userType = await GetUserType();

            if (userId == null)
                return Unauthorized();

            await _service.MarkAllRead(userId.Value, userType);

            return Ok();
        }
    }
}