using DAMS.DBAccess;
using DAMS.Models;
using System;
using System.Threading.Tasks;

namespace DAMS.Services
{
    public class TokenGenerationService
    {
        private readonly AppointmentRep _appointmentRep;

        public TokenGenerationService(AppointmentRep appointmentRep)
        {
            _appointmentRep = appointmentRep;
        }

        // 1. Generate Next Token (MAIN FUNCTION)
        public async Task<int> GenerateNextToken(int doctorId, DateOnly date)
        {
            int maxToken = await GetMaxToken(doctorId, date);
            return maxToken + 1;
        }

        // 2. Get Current Token (last assigned token)
        public async Task<int?> GetCurrentToken(int doctorId, DateOnly date)
        {
            return await _appointmentRep.GetLastTokenNumber(doctorId, date);
        }

        // 3. Get Max Token (core DB logic)
        public async Task<int> GetMaxToken(int doctorId, DateOnly date)
        {
            int? max = await _appointmentRep.GetMaxTokenNumber(doctorId, date);
            return max ?? 0;
        }

        // 4. Reserve Token (important for concurrency safety)
        public async Task<int> ReserveToken(int doctorId, DateOnly date)
        {
            lock (this) // simple protection (you can upgrade to DB transaction later)
            {
                int token = GetMaxToken(doctorId, date).Result + 1;
                return token;
            }
        }

        // 5. Reset Daily Tokens (optional cleanup logic)
        public async Task ResetDailyTokens(DateOnly date)
        {
            await _appointmentRep.ResetTokensByDate(date);
        }

        // 6. Validate Token (safety check)
        public async Task<bool> IsTokenValid(int doctorId, int tokenNumber, DateOnly date)
        {
            int max = await GetMaxToken(doctorId, date);
            return tokenNumber > 0 && tokenNumber <= max;
        }
    }
}