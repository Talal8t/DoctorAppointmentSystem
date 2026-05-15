namespace DAMS.Models
{
    public class QueueResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Appointment? Data { get; set; }
    }
}
