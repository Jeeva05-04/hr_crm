using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IAttendanceRepository
    {
        Task<(bool Success, string? Error)> CheckInAsync(
            int userId,
            string? ipAddress,
            double? latitude,
            double? longitude,
            string? deviceInfo
        );

        Task<bool> CheckOutAsync(int userId);

        Task<List<Attendance>> GetTodaySessionsAsync(int userId);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);

        Task<bool> UpdateLocationAsync(int userId, double latitude, double longitude);

        Task<List<Attendance>> GetActiveCheckInsAsync();

        Task<Attendance?> GetActiveCheckInAsync(int userId);

        Task<List<EmployeeLocationTrail>> GetLocationTrailAsync(int userId, DateTime date);
    }
}