namespace DAMS.Services
{
    public static class ChatIdGenerator
    {
        public static string PrivateChat(int user1Id, int user2Id)
        {
            return $"PRV_{Math.Min(user1Id, user2Id)}_{Math.Max(user1Id, user2Id)}";
        }

        public static string DoctorDailyBroadcast(int doctorId, DateOnly date)
        {
            return $"DOC_DAY_{doctorId}_{date:yyyyMMdd}";
        }

        public static string AdminBroadcast(string target)
        {
            return $"ADMIN_{target.ToUpperInvariant()}";
        }
    }
}
