using hr_crm.Entities;

namespace hr_crm.Repositories
{
    public interface IAttendanceRepository
    {
        Task MarkDailyAttendanceAsync();
        Task<bool> UpdateAttendanceAsync(int employeeId, string status);
        Task<List<Attendance>> GetTodayAttendanceAsync();
        Task<bool> CheckInAsync(int employeeId);
        Task<bool> CheckOutAsync(int employeeId);
        Task<Attendance?> GetTodayRecordAsync(int employeeId);
    }
}
