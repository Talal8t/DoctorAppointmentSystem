using Microsoft.AspNetCore.Mvc;
using DAMS.Services;

namespace DAMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminControlService _controlService;
        private readonly AdminRequestService _requestService;
        private readonly ReportService _reportService;

        public AdminController(
            AdminControlService controlService,
            AdminRequestService requestService,
            ReportService reportService)
        {
            _controlService = controlService;
            _requestService = requestService;
            _reportService = reportService;
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Dashboard()
        {
            var requests = await _requestService.GetPendingRequests();
            return View(requests);
        }

        // ================= REQUEST LIST PAGE =================
        public async Task<IActionResult> Requests(string status)
        {
            var requests = await _requestService.GetAllRequests();

            
            if (!string.IsNullOrEmpty(status))
            {
                requests = requests
                    .Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.CurrentFilter = status;

            return View(requests);
        }

        // ================= OPEN HANDLER PAGE =================
        [HttpPost]
        public IActionResult HandleRequest(int requestId)
        {
            return RedirectToAction("HandleRequestView", new { requestId });
        }

        public async Task<IActionResult> HandleRequestView(int requestId)
        {
            var request = await _requestService.GetById(requestId);
            return View("HandleRequest", request);
        }

        // ================= FINAL APPROVAL ACTIONS =================

        [HttpPost]
        public async Task<IActionResult> ConfirmStartOPD(int requestId, int doctorId)
        {
            await _controlService.StartOPD(doctorId);
            await _requestService.ApproveStartOPD(requestId, doctorId);

            return RedirectToAction("Requests");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmStopOPD(int requestId, int doctorId)
        {
            await _controlService.StopOPD(doctorId);
            await _requestService.ApproveStopOPD(requestId, doctorId);

            return RedirectToAction("Requests");
        }

        // ================= REJECT =================
        [HttpPost]
        public async Task<IActionResult> Reject(int requestId)
        {
            await _requestService.RejectRequest(requestId);
            return RedirectToAction("Requests");
        }

        // ================= OPD MONITORING (NEW) =================

        public async Task<IActionResult> OPDMonitor()
        {
            var doctors = await _controlService.GetActiveDoctorsFlow();
            return View(doctors);
        }

        public async Task<IActionResult> DoctorQueue(int doctorId)
        {
            var queue = await _controlService.GetDoctorQueue(doctorId);
            var current = queue.FirstOrDefault(x => x.Status == "InProgress");

            ViewBag.DoctorId = doctorId;
            ViewBag.Current = current;

            return View(queue);
        }

        public async Task<IActionResult> OPDSummary()
        {
            var summary = await _controlService.GetOPDSummaryFlow();
            return View(summary);
        }
        //=================Reports  =================
        // ================= REPORTS MAIN =================
        public IActionResult Reports()
        {
            return View();
        }

        // ================= DOCTOR REPORT =================
        public async Task<IActionResult> DoctorReports(string status)
        {
            var data = await _reportService.GetDoctorReports();

            if (status == "active")
                data = data.Where(d => d.IsActive).ToList();
            else if (status == "inactive")
                data = data.Where(d => !d.IsActive).ToList();

            return View(data);
        }

        // ================= PATIENT REPORT =================
        public async Task<IActionResult> PatientReports(string name)
        {
            var data = await _reportService.GetPatientReports();

            if (!string.IsNullOrEmpty(name))
                data = data.Where(p => p.PatientName.ToLower().Contains(name.ToLower())).ToList();

            return View(data);
        }

        // ================= APPOINTMENT REPORT =================
        public async Task<IActionResult> AppointmentReports(string status)
        {
            var data = await _reportService.GetAppointmentReports();

            if (!string.IsNullOrEmpty(status))
                data = data.Where(a => a.Status == status).ToList();

            return View(data);
        }
    }
}