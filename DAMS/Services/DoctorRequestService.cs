using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class DoctorRequestService
    {
        private readonly DoctorRequestRep _rep;

        public DoctorRequestService(DoctorRequestRep rep)
        {
            _rep = rep;
        }

        public async Task SendRequest(int doctorId, string type, int? targetDoctorId = null)
        {
            if (doctorId <= 0)
                throw new Exception("Invalid doctor");

            var req = new DoctorRequest
            {
                DoctorId = doctorId,
                RequestType = type,
                TargetDoctorId = targetDoctorId
            };

            await _rep.AddRequest(req);
        }
        public async Task<List<DoctorRequest>> GetAll()
        {
            return await _rep.GetAll();
        }

        public async Task<List<DoctorRequest>> GetPending()
        {
            return await _rep.GetPendingRequests();
        }

        public async Task Approve(int requestId)
        {
            await _rep.UpdateStatus(requestId, "Approved");
        }

        public async Task Reject(int requestId)
        {
            await _rep.UpdateStatus(requestId, "Rejected");
        }
        public async Task<DoctorRequest?> GetById(int requestId)
        {
            return await _rep.GetById(requestId);
        }
    }
}
