using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IAttendanceService
    {
        Task MarkDailyAttendanceAsync();
        Task<bool> UpdateAttendanceAsync(int employeeId, string status);
        Task<List<Attendance>> GetTodayAttendanceAsync();
        Task<bool> CheckInAsync(int employeeId);
        Task<bool> CheckOutAsync(int employeeId);
        Task<Attendance?> GetTodayRecordAsync(int employeeId);

        // ✅ Added for Past Attendance History
        Task<List<Attendance>> GetAttendanceHistoryAsync(int employeeId);
    }
}
