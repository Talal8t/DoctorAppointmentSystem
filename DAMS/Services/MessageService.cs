using DAMS.DBAccess;
using DAMS.Models;
using DAMS.ViewModels;

namespace DAMS.Services
{
    public class MessageService
    {
        private readonly MessageRep _repo;
        private readonly TcpClientService _tcpClient;
        private readonly AppointmentRep _appointmentRep;
        private readonly DoctorRep _doctorRep;
        private readonly PatientRep _patientRep;

        public MessageService(
            MessageRep repo,
            TcpClientService tcpClient,
            AppointmentRep appointmentRep,
            DoctorRep doctorRep,
            PatientRep patientRep)
        {
            _repo = repo;
            _tcpClient = tcpClient;
            _appointmentRep = appointmentRep;
            _doctorRep = doctorRep;
            _patientRep = patientRep;
        }

        public ChatViewModel BuildChat(int currentUserId, string role, int? doctorId, int? patientId, string? broadcastTarget)
        {
            var context = ResolveContext(currentUserId, role, doctorId, patientId, broadcastTarget);

            return new ChatViewModel
            {
                ChatId = context.ChatId,
                Title = context.Title,
                Mode = context.Mode,
                CurrentUserId = currentUserId,
                CurrentRole = role,
                DoctorId = doctorId,
                PatientId = patientId,
                BroadcastTarget = context.BroadcastTarget,
                Messages = _repo.GetChatHistory(context.ChatId, currentUserId, context.IsBroadcast)
            };
        }

        public async Task SendMessage(int currentUserId, string role, SendMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
                throw new Exception("Message cannot be empty");

            var context = ResolveContext(currentUserId, role, request.DoctorId, request.PatientId, request.BroadcastTarget);

            var message = new Message
            {
                ChatId = context.ChatId,
                SenderId = currentUserId,
                ReceiverId = context.ReceiverUserId,
                Content = request.Content.Trim(),
                MessageType = context.Mode,
                Status = "Sent",
                CreatedAt = DateTime.Now
            };

            if (context.Mode == "Private" && context.RecipientUserIds.Count == 1)
            {
                _repo.Save(message);
                message.ReceiverId = context.RecipientUserIds[0];
                await _tcpClient.SendMessage(message);
                return;
            }

            _repo.Save(message);

            foreach (var recipientUserId in context.RecipientUserIds)
            {
                _repo.Save(new Message
                {
                    ChatId = message.ChatId,
                    SenderId = message.SenderId,
                    ReceiverId = recipientUserId,
                    Content = message.Content,
                    MessageType = message.MessageType,
                    Status = message.Status,
                    CreatedAt = message.CreatedAt
                });
            }

            await _tcpClient.SendMessageToRecipients(message, context.RecipientUserIds);
        }

        public List<Message> GetChatHistory(string chatId)
        {
            return _repo.GetByChatId(chatId);
        }

        public List<Message> GetPrivateChat(int user1, int user2)
        {
            return _repo.GetPrivateChat(user1, user2);
        }

        public void MarkAsRead(int messageId)
        {
            _repo.UpdateStatus(messageId, "Read");
        }

        public List<Message> GetBroadcastMessagesForUser(int userId)
        {
            return _repo.GetBroadcastMessagesForUser(userId);
        }

        private ChatContext ResolveContext(int currentUserId, string role, int? doctorId, int? patientId, string? broadcastTarget)
        {
            role = role ?? string.Empty;
            broadcastTarget = broadcastTarget?.Trim().ToLowerInvariant();

            if (role == "Patient")
                return ResolvePatientContext(currentUserId, doctorId);

            if (role == "Doctor")
                return ResolveDoctorContext(currentUserId, patientId, broadcastTarget);

            if (role == "Admin")
                return ResolveAdminContext(currentUserId, broadcastTarget);

            throw new Exception("Login is required to use chat");
        }

        private ChatContext ResolvePatientContext(int currentUserId, int? doctorId)
        {
            if (doctorId == null)
                throw new Exception("Patient chat requires a doctor");

            var patient = _patientRep.GetPatientByUserId(currentUserId)
                ?? throw new Exception("Patient profile not found");

            var doctor = _doctorRep.GetDoctorById(doctorId.Value)
                ?? throw new Exception("Doctor not found");

            if (!_appointmentRep.HasBookedAppointmentBetweenDoctorAndPatient(doctor.DoctorId, patient.PatientId))
                throw new Exception("You can only message a doctor when you have a booked appointment");

            return new ChatContext
            {
                ChatId = ChatIdGenerator.PrivateChat(currentUserId, doctor.UserId),
                Title = $"Chat with Dr. {doctor.DoctorName}",
                Mode = "Private",
                ReceiverUserId = doctor.UserId,
                RecipientUserIds = new List<int> { doctor.UserId }
            };
        }

        private ChatContext ResolveDoctorContext(int currentUserId, int? patientId, string? broadcastTarget)
        {
            var doctor = _doctorRep.GetDoctorByUserId(currentUserId)
                ?? throw new Exception("Doctor profile not found");

            if (broadcastTarget == "todaypatients")
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var recipients = _appointmentRep.GetPatientIdsForDoctorOnDate(doctor.DoctorId, today)
                    .Select(_patientRep.GetUserIdByPatientId)
                    .Distinct()
                    .ToList();

                if (!recipients.Any())
                    throw new Exception("No booked patients found for today");

                return new ChatContext
                {
                    ChatId = ChatIdGenerator.DoctorDailyBroadcast(doctor.DoctorId, today),
                    Title = "Broadcast to today's patients",
                    Mode = "DoctorDailyBroadcast",
                    BroadcastTarget = "todayPatients",
                    IsBroadcast = true,
                    RecipientUserIds = recipients
                };
            }

            if (patientId == null)
                throw new Exception("Doctor private chat requires a patient");

            var patient = _patientRep.GetPatientById(patientId.Value)
                ?? throw new Exception("Patient not found");

            if (!_appointmentRep.HasBookedAppointmentBetweenDoctorAndPatient(doctor.DoctorId, patient.PatientId))
                throw new Exception("You can only message patients who have booked appointments with you");

            return new ChatContext
            {
                ChatId = ChatIdGenerator.PrivateChat(currentUserId, patient.UserId),
                Title = $"Chat with {patient.PatientName}",
                Mode = "Private",
                ReceiverUserId = patient.UserId,
                RecipientUserIds = new List<int> { patient.UserId }
            };
        }

        private ChatContext ResolveAdminContext(int currentUserId, string? broadcastTarget)
        {
            var target = string.IsNullOrWhiteSpace(broadcastTarget) ? "all" : broadcastTarget;

            List<int> recipients = target switch
            {
                "doctors" => _doctorRep.GetAllDoctors().Select(x => x.UserId).Distinct().ToList(),
                "patients" => _patientRep.GetAllPatients().Select(x => x.UserId).Distinct().ToList(),
                "all" => _doctorRep.GetAllDoctors().Select(x => x.UserId)
                    .Concat(_patientRep.GetAllPatients().Select(x => x.UserId))
                    .Distinct()
                    .ToList(),
                _ => throw new Exception("Admin broadcast target must be doctors, patients, or all")
            };

            if (!recipients.Any())
                throw new Exception("No users found for this broadcast");

            return new ChatContext
            {
                ChatId = ChatIdGenerator.AdminBroadcast(target),
                Title = target == "all" ? "Broadcast to doctors and patients" : $"Broadcast to {target}",
                Mode = $"AdminBroadcast:{target}",
                BroadcastTarget = target,
                IsBroadcast = true,
                RecipientUserIds = recipients
            };
        }

        private class ChatContext
        {
            public string ChatId { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Mode { get; set; } = "Private";
            public string? BroadcastTarget { get; set; }
            public bool IsBroadcast { get; set; }
            public int? ReceiverUserId { get; set; }
            public List<int> RecipientUserIds { get; set; } = new();
        }
    }
}
