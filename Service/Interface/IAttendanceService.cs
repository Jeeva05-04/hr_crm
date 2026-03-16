using hr_crm.Entities;
using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IAttendanceService
    {
        Task<(Attendance? Record, string? Error)> CheckInAsync(AttendanceCheckInDto dto, HttpContext httpContext);

        Task<bool> CheckOutAsync(int userId);

        Task<TimeSpan> CalculateTodayTotalHoursAsync(int userId);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);

        Task<bool> UpdateLocationAsync(int userId, double latitude, double longitude);

        Task<List<Attendance>> GetActiveCheckInsAsync();

        Task<Attendance?> GetActiveCheckInAsync(int userId);

        Task<List<EmployeeLocationTrail>> GetLocationTrailAsync(int userId, DateTime date);
    }
}