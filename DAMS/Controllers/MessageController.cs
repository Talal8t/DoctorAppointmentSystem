using DAMS.Services;
using DAMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DAMS.Controllers
{
    public class MessageController : Controller
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        public IActionResult Chat(int? doctorId, int? patientId, string? broadcastTarget)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || string.IsNullOrWhiteSpace(role))
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = _messageService.BuildChat(userId.Value, role, doctorId, patientId, broadcastTarget);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToRoleDashboard(role);
            }
        }

        public IActionResult Broadcasts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || string.IsNullOrWhiteSpace(role))
                return RedirectToAction("Login", "Auth");

            ViewBag.Role = role;
            return View(_messageService.GetBroadcastMessagesForUser(userId.Value));
        }

        public IActionResult Messages(int? doctorId, int? patientId, string? broadcastTarget)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || string.IsNullOrWhiteSpace(role))
                return Unauthorized();

            try
            {
                var model = _messageService.BuildChat(userId.Value, role, doctorId, patientId, broadcastTarget);
                return Json(ToMessageDtos(model.Messages));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult BroadcastMessages()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || string.IsNullOrWhiteSpace(role))
                return Unauthorized();

            var messages = _messageService.GetBroadcastMessagesForUser(userId.Value);
            return Json(ToMessageDtos(messages));
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || string.IsNullOrWhiteSpace(role))
                return Unauthorized("Login is required");

            try
            {
                await _messageService.SendMessage(userId.Value, role, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            if (role == "Doctor")
                return RedirectToAction("Dashboard", "Doctor");

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Dashboard", "Patient");
        }

        private static object ToMessageDtos(IEnumerable<DAMS.Models.Message> messages)
        {
            return messages.Select(message => new
            {
                messageId = message.MessageId,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                messageType = message.MessageType,
                status = message.Status,
                createdAt = message.CreatedAt.ToString("MMM dd, h:mm tt"),
                createdAtFull = message.CreatedAt.ToString("MMM dd, yyyy h:mm tt")
            });
        }
    }
}
