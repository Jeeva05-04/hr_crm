using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface ILeaveRepository
    {
        Task<List<Leave>> GetAllAsync();
        Task<List<Leave>> GetByUserIdAsync(int userId);
        Task<Leave?> GetByIdAsync(int leaveId);
        Task AddAsync(Leave leave);
        Task<bool> UpdateStatusAsync(int leaveId, string status, string approvedby);
        Task<bool> DeleteAsync(int leaveId);

        // Leave Balance
        Task<List<LeaveBalance>> GetBalanceByUserAsync(int userId, int year);
        Task<LeaveBalance?> GetBalanceAsync(int userId, string leaveType, int year);
        Task InitBalanceAsync(int userId, int year);
        Task<bool> DeductBalanceAsync(int userId, string leaveType, int days, int year);
        Task<bool> RestoreBalanceAsync(int userId, string leaveType, int days, int year);

        // Holiday Master
        Task<Holiday> AddHolidayAsync(Holiday holiday);
        Task<List<Holiday>> GetHolidaysAsync(int year);
        Task<bool> DeleteHolidayAsync(int id);
        Task<List<DateTime>> GetHolidayDatesAsync(int year);

        // Leave Calendar
        Task<List<Leave>> GetCalendarAsync(int month, int year);

        // Leave Encashment
        Task<LeaveEncashment> AddEncashmentAsync(LeaveEncashment encashment);
        Task<List<LeaveEncashment>> GetEncashmentsAsync(int userId);
    }
}
