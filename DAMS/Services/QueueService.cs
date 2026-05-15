using DAMS.Models;
using DAMS.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DAMS.Services
{
    public class QueueService
    {
        // private volatile bool _isOpdStarted = false;
        // private readonly DoctorService _doctorService;
        // private readonly AppointmentService _appointmentService;
        //private readonly OPDRepository _opdRepo;
        public readonly ConcurrentDictionary<int, bool> _doctorQueueStatus = new();
        public readonly ConcurrentDictionary<int, PriorityQueue<Appointment, int>> _doctorQueues
            = new();

        //public QueueService(OPDRepository opdRepository)
        //{
        //    _opdRepo = opdRepository;
        //}

        // admin




        // doctor
        public void StartDoctorQueue(int doctorId)
        {
            _doctorQueueStatus[doctorId] = true;
        }

        public void StopDoctorQueue(int doctorId)
        {
            _doctorQueueStatus[doctorId] = false;
        }

        //admin
        //public bool IsOPDRunning()
        //{
        //    return _isOpdStarted;
        //}

        public bool IsDoctorQueueRunning(int doctorId)
        {
            return _doctorQueueStatus.ContainsKey(doctorId)
                   && _doctorQueueStatus[doctorId];
        }


        public void EnqueueAppointment(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            if (appointment.Status != "Booked")
                throw new InvalidOperationException("Only booked appointments allowed.");

            var queue = _doctorQueues.GetOrAdd(
                appointment.DoctorId,
                _ => new PriorityQueue<Appointment, int>());

            lock (queue)
            {
                queue.Enqueue(appointment, GetPriority(appointment));
            }
        }

        public Appointment? GetNextPatient(int doctorId)
        {
            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return null;

            lock (queue)
            {
                return queue.Count > 0 ? queue.Peek() : null;
            }
        }

        public Appointment? CallNextPatient(int doctorId)
        {
            var accessResult = ValidateAccess(doctorId);

            if (!accessResult.Success)
                return null; // (we'll handle message in middleman)

            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return null;

            lock (queue)
            {
                if (queue.Count == 0)
                    return null;

                return queue.Dequeue();
            }
        }

        public List<Appointment> GetCurrentQueue(int doctorId)
        {
            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return new List<Appointment>();

            lock (queue)
            {
                return queue.UnorderedItems.Select(x => x.Element).ToList();
            }
        }

        public void Remove(int doctorId, int appointmentId)
        {
            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return;

            lock (queue)
            {
                var temp = new List<(Appointment, int)>();

                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();

                    if (item.AppointmentId != appointmentId)
                        temp.Add((item, GetPriority(item)));
                }

                foreach (var (app, priority) in temp)
                    queue.Enqueue(app, priority);
            }
        }

        public void LoadQueue(int doctorId, List<Appointment> appointments)
        {
            var queue = _doctorQueues.GetOrAdd(
                doctorId,
                _ => new PriorityQueue<Appointment, int>());

            lock (queue)
            {
                queue.Clear();

                foreach (var app in appointments)
                {
                    if (app.Status == "Booked" || app.Status == "InProgress")
                        queue.Enqueue(app, GetPriority(app));
                }
            }
        }

        public void ClearQueue(int doctorId)
        {
            if (_doctorQueues.TryGetValue(doctorId, out var queue))
            {
                lock (queue)
                {
                    queue.Clear();
                }
            }
        }
        public void UpdatePriority(int doctorId, Appointment appointment)
        {
            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return;

            lock (queue)
            {
                // 1. Remove old version
                var temp = new List<(Appointment, int)>();

                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();

                    if (item.AppointmentId != appointment.AppointmentId)
                        temp.Add((item, GetPriority(item)));
                }

                // 2. Update appointment priority already changed in DB
                temp.Add((appointment, GetPriority(appointment)));

                // 3. Rebuild
                foreach (var (app, priority) in temp)
                    queue.Enqueue(app, priority);
            }
        }
        public void RemoveAppointment(int doctorId, int appointmentId)
        {
            if (!_doctorQueues.TryGetValue(doctorId, out var queue))
                return;

            lock (queue)
            {
                var temp = new List<(Appointment, int)>();

                while (queue.Count > 0)
                {
                    var item = queue.Dequeue();

                    if (item.AppointmentId != appointmentId)
                        temp.Add((item, GetPriority(item)));
                }

                foreach (var (app, priority) in temp)
                    queue.Enqueue(app, priority);
            }
        }
        public void Reinsert(Appointment appointment)
        {
            RemoveAppointment(appointment.DoctorId, appointment.AppointmentId);
            EnqueueAppointment(appointment);
        }

        // ================= PRIORITY =================

        private int GetPriority(Appointment appointment)
        {
            int priorityWeight = GetPriorityWeight(appointment);

            return (priorityWeight * 100000) + appointment.TokenNumber;
        }

        private static int GetPriorityWeight(Appointment appointment)
        {
            return appointment.Priority switch
            {
                PriorityLevel.Emergency => 0,
                PriorityLevel.Urgent => 1,
                PriorityLevel.Normal => 2,
                _ => 2
            };
        }

        // CLEAR ALL
        public void ClearAllQueues()
        {
            foreach (var key in _doctorQueues.Keys)
            {
                ClearQueue(key);
            }
        }

        // CLEAR SPECIFIC
        public void ClearDoctorQueueByAdmin(int doctorId)
        {
            ClearQueue(doctorId);
        }
        public bool Exists(int doctorId)
        {
            return _doctorQueues.ContainsKey(doctorId);
        }
        public (bool Success, string Message) ValidateAccess(int doctorId)
        {
            //if (!_opdRepo.IsOPDStarted(doctorId))
            //    return (false, "OPD is not started by admin.");

            if (!_doctorQueueStatus.ContainsKey(doctorId) || !_doctorQueueStatus[doctorId])
                return (false, "Doctor queue is not active.");

            return (true, "");
        }
        public int GetActiveDoctorCount()
        {
            return _doctorQueueStatus.Count(x => x.Value);
        }
        public int GetTotalQueues()
        {
            return _doctorQueues.Count;
        }
        public int GetTotalPatientsInQueues()
        {
            return _doctorQueues.Sum(x => x.Value.Count);
        }
    }

        public enum PriorityLevel
    {
        Normal = 0,
        Urgent = 1,
        Emergency = 2
    }
}