using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface ILeaveService
    {
        Task<(bool Success, string? Error)> ApplyLeaveAsync(LeaveCreateDto dto);
        Task<List<LeaveResponseDto>> GetAllLeavesAsync();
        Task<List<LeaveResponseDto>> GetLeavesByUserAsync(int userId);
        Task<(bool Success, string? Error)> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto);
        Task<bool> DeleteLeaveAsync(int leaveId);

        // Leave Balance
        Task<List<LeaveBalance>> GetBalanceAsync(int userId);

        // Holiday Master
        Task<Holiday> AddHolidayAsync(HolidayCreateDto dto, int createdBy);
        Task<List<Holiday>> GetHolidaysAsync(int year);
        Task<bool> DeleteHolidayAsync(int id);

        // Leave Calendar
        Task<object> GetCalendarAsync(int month, int year);

        // Leave Encashment
        Task<(LeaveEncashment? Result, string? Error)> ProcessEncashmentAsync(int userId, string userName, int year, int processedBy);
        Task<List<LeaveEncashment>> GetEncashmentsAsync(int userId);
    }
}
