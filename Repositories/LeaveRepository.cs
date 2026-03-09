using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;


namespace hr_crm.Repositories
{
    public class LeaveRepository : ILeaveRepository  // ← make sure this is there
    {
        private readonly AppDbContext _context;

            public LeaveRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<List<Leave>> GetAllAsync()
            {
                return await _context.Leaves
                    .OrderByDescending(l => l.AppliedOn)
                    .ToListAsync();
            }

            public async Task<List<Leave>> GetByEmployeeIdAsync(int employeeId)
            {
                return await _context.Leaves
                    .Where(l => l.EmployeeId == employeeId)
                    .OrderByDescending(l => l.AppliedOn)
                    .ToListAsync();
            }

            public async Task<Leave?> GetByIdAsync(int leaveId)
            {
                return await _context.Leaves
                    .FirstOrDefaultAsync(l => l.LeaveId == leaveId);
            }

            public async Task AddAsync(Leave leave)
            {
                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateStatusAsync(int leaveId, string status,string approvedby)
            {
                var leave = await _context.Leaves.FindAsync(leaveId);
                if (leave == null) return false;

                leave.Status = status;
                leave.ApprovedBY = approvedby;
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task<bool> DeleteAsync(int leaveId)
            {
                var leave = await _context.Leaves.FindAsync(leaveId);
                if (leave == null) return false;

                _context.Leaves.Remove(leave);
                await _context.SaveChangesAsync();
                return true;
            }
        
    }
}

