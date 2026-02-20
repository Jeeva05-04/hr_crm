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

        public Task<bool> CheckInAsync(int userId)
            => _repo.CheckInAsync(userId);

        public Task<bool> CheckOutAsync(int userId)
            => _repo.CheckOutAsync(userId);

        public Task<Attendance?> GetTodayRecordAsync(int userId)
            => _repo.GetTodayRecordAsync(userId);

        public Task<bool> UpdateAttendanceAsync(int userId, string status)
            => _repo.UpdateAttendanceAsync(userId, status);

        public Task<List<Attendance>> GetAttendanceHistoryAsync(int userId)
            => _repo.GetAttendanceHistoryAsync(userId);
    }
}
