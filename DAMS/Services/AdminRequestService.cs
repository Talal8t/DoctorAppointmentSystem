using DAMS.Models;

namespace DAMS.Services
{
    public class AdminRequestService
    {
        private readonly DoctorRequestService _requestRepo;
        private readonly AdminControlService _adminControl;
        private readonly MiddleMan _middleMan;

        public AdminRequestService(
            DoctorRequestService requestRepo,
            AdminControlService adminControl,
            MiddleMan middleMan)
        {
            _requestRepo = requestRepo;
            _adminControl = adminControl;
            _middleMan = middleMan;
        }
        public async Task<List<DoctorRequest>> GetAllRequests()
        {
            var list= await _requestRepo.GetAll();
            if (list != null) { 
            
                foreach (var req in list)
                {
                    var doctor =  _middleMan.GetDoctorFlow(req.DoctorId);
                    req.DoctorName = doctor?.DoctorName ?? "Unknown Doctor";
                }
                return list;
            }
            return await _requestRepo.GetAll();

        }
        // ================= GET PENDING =================

        public async Task<List<DoctorRequest>> GetPendingRequests()
        {
            return await _requestRepo.GetPending();
        }

        // ================= APPROVE START OPD =================

        public async Task ApproveStartOPD(int requestId, int doctorId)
        {
            // optional: you can log doctorId later

            await _adminControl.StartOPD(doctorId);

            await _requestRepo.Approve(requestId);
        }

        // ================= APPROVE STOP OPD =================

        public async Task ApproveStopOPD(int requestId, int doctorId)
        {
            await _adminControl.StopOPD(doctorId);

            await _requestRepo.Approve(requestId);
        }

        // ================= REJECT REQUEST =================

        public async Task RejectRequest(int requestId)
        {
            await _requestRepo.Reject(requestId);
        }
        public async Task<DoctorRequest?> GetById(int requestId)
        {
            return await _requestRepo.GetById(requestId);
        }
    }
}