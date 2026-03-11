using hr_crm.Entities;
using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IAttendanceService
    {
        Task<Attendance> CheckInAsync(AttendanceCheckInDto dto, HttpContext httpContext);  

        Task<bool> CheckOutAsync(int userId);

        Task<TimeSpan> CalculateTodayTotalHoursAsync(int userId);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);
    }
}