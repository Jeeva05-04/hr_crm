using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface ILeaveService
    {
        Task ApplyLeaveAsync(LeaveCreateDto dto);
        Task<List<LeaveResponseDto>> GetAllLeavesAsync();
        Task<List<LeaveResponseDto>> GetLeavesByUserAsync(int userId);
        Task<bool> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto);
        Task<bool> DeleteLeaveAsync(int leaveId);
    }
}