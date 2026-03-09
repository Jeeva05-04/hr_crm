using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class LeaveService :ILeaveService
    {
            private readonly ILeaveRepository _repo;

            public LeaveService(ILeaveRepository repo)
            {
                _repo = repo;
            }

            public async Task<bool> ApplyLeaveAsync(LeaveCreateDto dto)
            {
                var leave = new Leave
                {
                    EmployeeId = dto.EmployeeId,
                    LeaveType = dto.LeaveType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Reason = dto.Reason,
                    Status = dto.Status,
                    ApprovedBY = dto.ApprovedBY,
                    AppliedOn = DateTime.UtcNow
                };

                await _repo.AddAsync(leave);
                return true;
            }

            public Task<List<Leave>> GetAllLeavesAsync()
                => _repo.GetAllAsync();

            public Task<List<Leave>> GetLeavesByEmployeeAsync(int employeeId)
                => _repo.GetByEmployeeIdAsync(employeeId);

            public Task<bool> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto)
                => _repo.UpdateStatusAsync(leaveId, dto.Status,dto.ApprovedBY);

            public Task<bool> DeleteLeaveAsync(int leaveId)
                => _repo.DeleteAsync(leaveId);
        
    }
}

