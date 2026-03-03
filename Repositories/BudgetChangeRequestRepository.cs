using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class BudgetChangeRequestRepository : IBudgetChangeRequestRepository
    {
        private readonly AppDbContext _context;

        public BudgetChangeRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ CREATE
        public async Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request)
        {
            request.Status = "Pending";
            request.RequestDate = DateTime.UtcNow;

            _context.BudgetChangeRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        // ✅ GET ALL
        public async Task<List<BudgetChangeRequest>> GetAllAsync()
        {
            return await _context.BudgetChangeRequests.ToListAsync();
        }

        // ✅ GET BY ID
        public async Task<BudgetChangeRequest?> GetByIdAsync(int id)
        {
            return await _context.BudgetChangeRequests
                .FirstOrDefaultAsync(r => r.BudgetChangeRequestId == id);
        }

        // ✅ APPROVE
        public async Task<bool> ApproveAsync(int requestId, int approverId)
        {
            var request = await _context.BudgetChangeRequests
                .FirstOrDefaultAsync(r => r.BudgetChangeRequestId == requestId);

            if (request == null || request.Status != "Pending")
                return false;

            request.Status = "Approved";
            request.ApprovedBy = approverId;
            request.ApprovedDate = DateTime.UtcNow;

            // 🔥 Update Department Budget
            var budget = await _context.DepartmentBudgets
                .FirstOrDefaultAsync(b => b.DepartmentId == request.DepartmentId);

            if (budget != null)
            {
                budget.TotalAnnualBudget += request.RequestedAmount;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ REJECT
        public async Task<bool> RejectAsync(int requestId, int approverId)
        {
            var request = await _context.BudgetChangeRequests
                .FirstOrDefaultAsync(r => r.BudgetChangeRequestId == requestId);

            if (request == null || request.Status != "Pending")
                return false;

            request.Status = "Rejected";
            request.ApprovedBy = approverId;
            request.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}