using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;

        public AttendanceService(IAttendanceRepository repo)
        {
            _repo = repo;
        }

        // =========================================
        // ✅ Check-In (Create new session)
        // =========================================
        public async Task<bool> CheckInAsync(int userId)
        {
            return await _repo.CheckInAsync(userId);
        }

        // =========================================
        // ✅ Check-Out (Close open session)
        // =========================================
        public async Task<bool> CheckOutAsync(int userId)
        {
            return await _repo.CheckOutAsync(userId);
        }

        // =========================================
        // ✅ Calculate Today's Total Hours
        // =========================================
        public async Task<TimeSpan> CalculateTodayTotalHoursAsync(int userId)
        {
            var sessions = await _repo.GetTodaySessionsAsync(userId);

            TimeSpan total = TimeSpan.Zero;

            foreach (var session in sessions)
            {
                if (session.CheckOutTime != null)
                {
                    total += session.CheckOutTime.Value - session.CheckInTime;
                }
            }

            return total;
        }

        // =========================================
        // ✅ Get Full History
        // =========================================
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
        {
            return await _repo.GetAttendanceHistoryAsync(userId);
        }
    }
}