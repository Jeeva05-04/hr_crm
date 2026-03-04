using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class DepartmentBudgetRepository : IDepartmentBudgetRepository
    {
        private readonly AppDbContext _context;

        public DepartmentBudgetRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL BUDGETS
        public async Task<List<DepartmentBudget>> GetAllAsync()
        {
            return await _context.DepartmentBudgets
                .ToListAsync();
        }

        // ✅ GET BY ID
        public async Task<DepartmentBudget?> GetByIdAsync(int id)
        {
            return await _context.DepartmentBudgets
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // ✅ GET BY DEPARTMENT ID
        public async Task<List<DepartmentBudget>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _context.DepartmentBudgets
                .Where(b => b.DepartmentId == departmentId)
                .ToListAsync();
        }

        // ✅ CREATE
        public async Task<DepartmentBudget> CreateAsync(DepartmentBudget budget)
        {
            budget.Status = "Draft";
            budget.CreatedDate = DateTime.UtcNow;

            _context.DepartmentBudgets.Add(budget);
            await _context.SaveChangesAsync();

            return budget;
        }

        // ✅ APPROVE BY HEAD
        public async Task<bool> ApproveByHeadAsync(int budgetId, int headUserId)
        {
            var budget = await _context.DepartmentBudgets.FindAsync(budgetId);

            if (budget == null || budget.Status != "Draft")
                return false;

            budget.Status = "HeadApproved";
            budget.HeadApprovedBy = headUserId;
            budget.HeadApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var budget = await _context.DepartmentBudgets
                .FirstOrDefaultAsync(x => x.Id == id);   // ✅ FIXED HERE

            if (budget == null)
                return false;

            _context.DepartmentBudgets.Remove(budget);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ APPROVE BY FINANCE
        public async Task<bool> ApproveByFinanceAsync(int budgetId, int financeUserId, decimal approvedAmount)
        {
            var budget = await _context.DepartmentBudgets.FindAsync(budgetId);

            if (budget == null || budget.Status != "HeadApproved")
                return false;

            budget.Status = "FinanceApproved";
            budget.FinanceApprovedBy = financeUserId;
            budget.FinanceApprovedDate = DateTime.UtcNow;
            budget.ApprovedAmount = approvedAmount;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}