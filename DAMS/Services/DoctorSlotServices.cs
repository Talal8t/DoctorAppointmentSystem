using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class DoctorSlotServices
    {
        private readonly DoctorSlotRep _doctorSlotRep;
        private readonly DoctorService _doctorService;

        public DoctorSlotServices(DoctorSlotRep doctorSlotRep, DoctorService doctorService)
        {
            _doctorSlotRep = doctorSlotRep;
            _doctorService = doctorService;
        }
        public async Task<string> AddDoctorSlotAsync(DoctorSlot slot)
        {
            if (await _doctorSlotRep.AddDoctorSlot(slot) > 0)
            {
                return "Doctor slot created successfully.";
            }
            else
            {
                return "Failed to create doctor slot.";
            }
        }
        public async Task<List<DoctorSlot>> GetDoctorSlotsByDoctorIdAsync(int doctorId)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            return await _doctorSlotRep.GetDoctorSlotsByDoctorId(doctorId);
        }
        public async Task<string> UpdateDoctorSlotStatusAsync(int slotId, string newStatus)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            if (newStatus != "Available" && newStatus != "Booked")
            {
                return "Invalid status. Status must be 'Available' or 'Booked'.";
            }
            slot.Status = newStatus;
            if (await _doctorSlotRep.UpdateDoctorSlot(slot) > 0)
            {
                return "Doctor slot status updated successfully.";
            }
            else
            {
                return "Failed to update doctor slot status.";
            }
        }
        public async Task<string> DeleteDoctorSlotAsync(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            if (await _doctorSlotRep.DeleteDoctorSlot(slotId) > 0)
            {
                return "Doctor slot deleted successfully.";
            }
            else
            {
                return "Failed to delete doctor slot.";
            }
        }
        //Read operations
        public async Task<List<DoctorSlot>> GetSlotByDateandDoctor(int doctorId, DateTime date)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            if (await _doctorSlotRep.GetSlotsByDate(date) == null)
            {
                throw new Exception("No slots found for the given date.");
            }
            return await _doctorSlotRep.GetSlotsByDoctorIdAndDate(doctorId, date);
        }

        public async Task<List<DoctorSlot>> GetAvailableSlotsByDate(DateTime date)
        {
            var slots = await _doctorSlotRep.GetAvailableSlotsByDate(date);
            if (slots == null || !slots.Any())
            {
                throw new Exception("No slots found for the given date.");
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetAvailableSlotsByDateandDoctor(int doctorId, DateTime date)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetAvailableSlotsByDoctorIdandDate(doctorId, date);
            if (slots == null || !slots.Any())
            {
                return null;
            }
            return slots;
        }
        public async Task<List<DoctorSlot>> GetAllDoctorSlotsAsync()
        {
            return await _doctorSlotRep.GetAllDoctorSlots();
        }
        public async Task<DoctorSlot> GetDoctorSlotByIdAsync(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                throw new Exception("Doctor slot not found.");
            }
            return slot;
        }
        public async Task<DoctorSlot?> GetNextActiveSlot(int doctorId)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetSlotsByDoctorIdAndStatus(doctorId, "Available");

            var nextActiveSlot = slots
                .Where(s => s.SlotDate >= DateTime.Today)
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.SlotTime)
                .FirstOrDefault();
            return nextActiveSlot;
        }
        public async Task<List<DoctorSlot>> GetSlotsByStatus(int doctorId, string status)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            return await _doctorSlotRep.GetSlotsByDoctorIdAndStatus(doctorId, status);
        }
        public async Task<List<DoctorSlot>> GetSlotsByDateRange(int doctorId, DateTime startDate, DateTime endDate)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetDoctorSlotsByDoctorId(doctorId);
            return slots.Where(s => s.SlotDate >= startDate && s.SlotDate <= endDate).ToList();
        }
        public async Task<List<DoctorSlot>> GetSlotsByTimeRange(int doctorId, TimeSpan startTime, TimeSpan endTime)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetDoctorSlotsByDoctorId(doctorId);
            return slots.Where(s => s.SlotTime >= startTime && s.SlotTime <= endTime).ToList();
        }
        public async Task<List<DoctorSlot>> GetSlotsByDateAndTimeRange(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetDoctorSlotsByDoctorId(doctorId);
            return slots.Where(s => s.SlotDate.Date == date.Date && s.SlotTime >= startTime && s.SlotTime <= endTime).ToList();
        }
        public async Task<List<DoctorSlot>> GetSlotsByStatusAndDate(int doctorId, string status, DateTime date)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetSlotsByDoctorIdAndStatus(doctorId, status);
            return slots.Where(s => s.SlotDate.Date == date.Date).ToList();
        }
        public async Task<List<DoctorSlot>> GetSlotsByStatusAndTime(int doctorId, string status, TimeSpan time)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            var slots = await _doctorSlotRep.GetSlotsByDoctorIdAndStatus(doctorId, status);
            return slots.Where(s => s.SlotTime == time).ToList();
        }
        public async Task<List<DoctorSlot>> GetBookedSlotsByDoctorIdAsync(int doctorId)
        {
            if (!await _doctorService.DoctorExists(doctorId))
            {
                throw new Exception("Doctor not found.");
            }
            return await _doctorSlotRep.GetSlotsByDoctorIdAndStatus(doctorId, "Booked");
        }
        // Additional methods for more complex queries can be added here business logic
        public async Task<bool> IsSlotAvailable(int slodId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slodId);
            if (slot == null)
            {
                throw new Exception("Doctor slot not found.");
            }
            return slot.Status == "Available";
        }
        public async Task<string> BookSlot(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            if (slot.Status == "Available")
            {
                slot.Status = "Booked";
            }
            if (await _doctorSlotRep.UpdateDoctorSlot(slot) > 0)
            {
                return $"Doctor slot status toggled to {slot.Status} successfully.";
            }
            else
            {
                return "Failed to toggle doctor slot status.";
            }
        }
        public async Task<string> ToggleSlotStatus(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            slot.Status = slot.Status == "Available" ? "Booked" : "Available";
            if (await _doctorSlotRep.UpdateDoctorSlot(slot) > 0)
            {
                return $"Doctor slot status toggled to {slot.Status} successfully.";
            }
            else
            {
                return "Failed to toggle doctor slot status.";
            }
        }
        public async Task<string> ReleaseSlot(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            if (slot.Status == "Booked")
            {
                slot.Status = "Available";
            }
            if (await _doctorSlotRep.UpdateDoctorSlot(slot) > 0)
            {
                return $"Doctor slot status toggled to {slot.Status} successfully.";
            }
            else
            {
                return "Failed to toggle doctor slot status.";
            }
        }

        
        public async Task<string> CloseSlot(int slotId)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);
            if (slot == null)
            {
                return "Doctor slot not found.";
            }
            slot.Status = "Closed";
            if (await _doctorSlotRep.UpdateDoctorSlot(slot) > 0)
            {
                return $"Doctor slot status updated to {slot.Status} successfully.";
            }
            else
            {
                return "Failed to update doctor slot status.";
            }
        }

        public async Task ValidateSlotForBooking(int doctorId, int slotId, DateTime date)
        {
            var slot = await _doctorSlotRep.GetDoctorSlotById(slotId);

            if (slot == null)
                throw new Exception("Slot not found");

            if (slot.DoctorId != doctorId)
                throw new Exception("Slot mismatch");

            if (slot.Status != "Available")
                throw new Exception("Slot not available");

            if (slot.SlotDate.Date != date.Date)
                throw new Exception("Invalid date");

            if (slot.SlotDate.Date < DateTime.Today)
                throw new Exception("Slot expired");
        }
        public async Task<bool> IsSlotAvailable(int doctorId, int slotId, DateTime date)
        {
            try
            {
                await ValidateSlotForBooking(doctorId, slotId, date);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
