using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface ILeaveRepository
    {


            Task<List<Leave>> GetAllAsync();
            Task<List<Leave>> GetByEmployeeIdAsync(int employeeId);
            Task<Leave?> GetByIdAsync(int leaveId);
            Task AddAsync(Leave leave);
            Task<bool> UpdateStatusAsync(int leaveId, string status, string approvedby);
            Task<bool> DeleteAsync(int leaveId);
        
    }
}

