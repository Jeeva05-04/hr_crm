using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IAttendanceRepository
    {
        Task<bool> CheckInAsync(
            int userId,
            string? ipAddress,
            double? latitude,
            double? longitude,
            string? deviceInfo
        );

        Task<bool> CheckOutAsync(int userId);

        Task<List<Attendance>> GetTodaySessionsAsync(int userId);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);
    }
}