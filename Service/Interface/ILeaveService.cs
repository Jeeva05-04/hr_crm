using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface ILeaveService
    {
       
            Task<bool> ApplyLeaveAsync(LeaveCreateDto dto);
            Task<List<Leave>> GetAllLeavesAsync();
            Task<List<Leave>> GetLeavesByEmployeeAsync(int employeeId);
            Task<bool> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto);
            Task<bool> DeleteLeaveAsync(int leaveId);
        
    }
}

