namespace DAMS.Services
{
    public class QueueInitializerService
    {
        private readonly AppointmentService _appointmentService;
        private readonly QueueService _queueService;

        public QueueInitializerService(
            AppointmentService appointmentService,
            QueueService queueService)
        {
            _appointmentService = appointmentService;
            _queueService = queueService;
        }

        public async Task InitializeAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var appointments = await _appointmentService.GetAppointmentsByDate(today);

            var grouped = appointments
                .Where(a => a.Status == "Booked" || a.Status == "InProgress")
                .GroupBy(a => a.DoctorId);

            foreach (var group in grouped)
            {
                _queueService.LoadQueue(group.Key, group.ToList());
            }
        }
    }
}
