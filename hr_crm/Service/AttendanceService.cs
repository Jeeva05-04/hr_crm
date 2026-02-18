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

        public Task MarkDailyAttendanceAsync()
            => _repo.MarkDailyAttendanceAsync();

        public Task<bool> UpdateAttendanceAsync(int employeeId, string status)
            => _repo.UpdateAttendanceAsync(employeeId, status);

        public Task<List<Attendance>> GetTodayAttendanceAsync()
            => _repo.GetTodayAttendanceAsync();

        public Task<bool> CheckInAsync(int employeeId)
            => _repo.CheckInAsync(employeeId);

        public Task<bool> CheckOutAsync(int employeeId)
            => _repo.CheckOutAsync(employeeId);

        public Task<Attendance?> GetTodayRecordAsync(int employeeId)
            => _repo.GetTodayRecordAsync(employeeId);

        // ✅ Added for Attendance History
        public Task<List<Attendance>> GetAttendanceHistoryAsync(int employeeId)
            => _repo.GetAttendanceHistoryAsync(employeeId);
    }
}
