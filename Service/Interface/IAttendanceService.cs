using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IAttendanceService
    {
        Task<bool> CheckInAsync(int userId);

        Task<bool> CheckOutAsync(int userId);

        Task<TimeSpan> CalculateTodayTotalHoursAsync(int userId);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId);
    }
}