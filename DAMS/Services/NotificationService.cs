using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class NotificationService
    {
        private readonly NotificationRep _rep;
        private readonly DoctorRep _doctorRep;
        private readonly PatientRep _patientRep;

        public NotificationService(NotificationRep rep, DoctorRep doctorRep, PatientRep patientRep)
        {
            _rep = rep;
            _doctorRep = doctorRep;
            _patientRep = patientRep;
        }

        // ================= SEND NOTIFICATION =================
        public async Task SendNotification(int userId, string userType, string message)
        {
            if (userId <= 0)
                throw new Exception("Invalid user");

            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("Message cannot be empty");

            if (message.Length > 250)
                throw new Exception("Message too long");

            // prevent spam duplicates (simple check)
            var last = await _rep.GetUnreadNotifications(userId, userType);

            if (last.Any() && last.First().Message == message)
                return;

            await _rep.AddNotification(userId, userType, message);
        }

        // ================= GET UNREAD =================
        public async Task<List<Notification>> GetUnread(int userId, string userType)
        {
            if (userId <= 0)
                throw new Exception("Invalid user");

            return await _rep.GetUnreadNotifications(userId, userType);
        }

        // ================= MARK ALL AS READ =================
        public async Task MarkAllRead(int userId, string userType)
        {
            if (userId <= 0)
                throw new Exception("Invalid user");

            var list = await _rep.GetUnreadNotifications(userId, userType);

            if (!list.Any())
                return;

            await _rep.MarkNotificationsRead(userId, userType);
        }

        // ================= COUNT UNREAD =================
        public async Task<int> GetUnreadCount(int userId, string userType)
        {
            if (userId <= 0)
                return 0;

            return await _rep.GetUnreadCount(userId, userType);
        }

        // ================= DOMAIN NOTIFICATIONS =================

        // 🔔 Doctor gets notified when patient books
        public async Task NotifyDoctorAppointmentBooked(int doctorId, int patientId)
        {
            if (doctorId <= 0 || patientId <= 0)
                throw new Exception("Invalid data");

            string message = $"Patient {patientId} booked an appointment";
            int userid=_doctorRep.GetUserIdByDoctorId(doctorId);
            await SendNotification(userid, "Doctor", message);
        }

        // 🔔 Patient gets notified when called
        public async Task NotifyPatientTurn(int patientId, int token)
        {
            if (patientId <= 0)
                throw new Exception("Invalid patient");

            string message = $"Your turn! Token #{token} please proceed";
            int userid = _patientRep.GetUserIdByPatientId(patientId);
            await SendNotification(userid, "Patient", message);
        }

        // 🔔 Patient appointment completed
        public async Task NotifyAppointmentCompleted(int patientId)
        {
            if (patientId <= 0)
                throw new Exception("Invalid patient");

            string message = $"Your appointment has been completed";
            int userid = _patientRep.GetUserIdByPatientId(patientId);

            await SendNotification(userid, "Patient", message);
        }

        // 🔔 Patient missed appointment
        public async Task NotifyAppointmentMissed(int patientId)
        {
            if (patientId <= 0)
                throw new Exception("Invalid patient");

            string message = $"You missed your appointment";
            int userid=_patientRep.GetUserIdByPatientId(patientId);
            await SendNotification(userid, "Patient", message);
        }
    }
}