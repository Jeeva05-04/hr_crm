using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IAttendanceService
    {
        Task<bool> CheckInAsync(int userId);

        Task<bool> CheckOutAsync(int userId);

        Task<Attendance?> GetTodayRecordAsync(int userId);

        Task<bool> UpdateAttendanceAsync(int userId, string status);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);
    }
}

