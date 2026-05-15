using DAMS.Models;
using DAMS.Repositories;
using DAMS.Services;

public class AdminControlService
{
    private readonly QueueService _queueService;
    private readonly OPDRepository _opdRepo;
    private readonly AppointmentService _appointmentService;
    private readonly DoctorService _doctorService;

    public AdminControlService(
        QueueService queueService,
        OPDRepository opdRepo,
        AppointmentService appointmentService,
        DoctorService doctorService )
    {
        _queueService = queueService;
        _opdRepo = opdRepo;
        _appointmentService = appointmentService;
        _doctorService = doctorService;
    }

    // ================= GLOBAL OPD CONTROL =================

    public async Task StartOPD(int doctorId)
    {
         _opdRepo.StartOPD(doctorId);

        _queueService.StartDoctorQueue(doctorId); // ✅ IMPORTANT FIX

        var appointments = await _appointmentService.GetAppointmentsByDoctorId(doctorId);

        var today = appointments
            .Where(a => a.Status == "Booked" || a.Status == "InProgress")
            .ToList();

        _queueService.LoadQueue(doctorId, today);
    }

    public async Task StopOPD(int doctorId)
    {
         _opdRepo.StopOPD(doctorId);

        _queueService.StopDoctorQueue(doctorId); // ✅ IMPORTANT FIX

        _queueService.ClearQueue(doctorId);
    }

    public  bool IsOPDRunning(int doctorId)
    {
        return  _opdRepo.IsOPDStarted(doctorId);
    }

    // ================= DOCTOR CONTROL =================

    public void StartDoctorQueue(int doctorId)
    {
        _queueService.StartDoctorQueue(doctorId);
    }

    public void StopDoctorQueue(int doctorId)
    {
        _queueService.StopDoctorQueue(doctorId);
    }

    // ================= QUEUE CONTROL =================

    public void ClearAllQueues()
    {
        _queueService.ClearAllQueues();
    }

    public void ClearDoctorQueue(int doctorId)
    {
        _queueService.ClearDoctorQueueByAdmin(doctorId);
    }
    public async Task<List<Doctor>> GetActiveDoctorsFlow()
    {
        return _doctorService.GetAllDoctors()
            .Where(d => _queueService.IsDoctorQueueRunning(d.DoctorId))
            .ToList();
    }
    public async Task<List<Appointment>> GetDoctorQueue(int doctorId)
    {
        return _queueService.GetCurrentQueue(doctorId);
    }
    public async Task<object> GetOPDSummaryFlow()
    {
        return new
        {
            ActiveDoctors = _queueService.GetActiveDoctorCount(),
            TotalQueues = _queueService.GetTotalQueues(),
            TotalPatients = _queueService.GetTotalPatientsInQueues()
        };
    }
}