using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IAttendanceRepository
    {
        Task MarkDailyAttendanceAsync();
        Task<bool> UpdateAttendanceAsync(int userId, string status);
        Task<List<Attendance>> GetTodayAttendanceAsync();
        Task<bool> CheckInAsync(int userId);
        Task<bool> CheckOutAsync(int userId);
        Task<Attendance?> GetTodayRecordAsync(int userId);
        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);
    }
}
