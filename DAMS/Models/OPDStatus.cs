namespace DAMS.Models
{
    public class OPDStatus
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public bool IsOPDStarted { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
