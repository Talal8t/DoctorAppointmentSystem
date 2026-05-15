namespace DAMS.Models
{
    public class DoctorRequest
    {
        public int RequestId { get; set; }
        public int DoctorId { get; set; }
        public string RequestType { get; set; } // StartOPD, StopOPD, ClearQueue, etc
        public int? TargetDoctorId { get; set; } // for specific doctor actions
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; }
        public string DoctorName { get; set; }
    }
}
