using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _repo;

        public LeaveService(ILeaveRepository repo)
        {
            _repo = repo;
        }

        public async Task ApplyLeaveAsync(LeaveCreateDto dto)
        {
            var leave = new Leave
            {
                UserId = dto.UserId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                ApprovedBY = dto.ApprovedBY,
                AppliedOn = DateTime.UtcNow
            };

            await _repo.AddAsync(leave);
        }

        public async Task<List<LeaveResponseDto>> GetAllLeavesAsync()
        {
            var leaves = await _repo.GetAllAsync();

            return leaves.Select(l => new LeaveResponseDto
            {
                LeaveId = l.LeaveId,
                UserId = l.UserId,
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                ApprovedBY = l.ApprovedBY,
                AppliedOn = l.AppliedOn
            }).ToList();
        }

        public async Task<List<LeaveResponseDto>> GetLeavesByUserAsync(int userId)
        {
            var leaves = await _repo.GetByUserIdAsync(userId);

            return leaves.Select(l => new LeaveResponseDto
            {
                LeaveId = l.LeaveId,
                UserId = l.UserId,
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                ApprovedBY = l.ApprovedBY,
                AppliedOn = l.AppliedOn
            }).ToList();
        }

        public Task<bool> UpdateLeaveStatusAsync(int leaveId, LeaveStatusDto dto)
            => _repo.UpdateStatusAsync(leaveId, dto.Status, dto.ApprovedBY);

        public Task<bool> DeleteLeaveAsync(int leaveId)
            => _repo.DeleteAsync(leaveId);
    }
}