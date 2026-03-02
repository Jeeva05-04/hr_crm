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

        public async Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request)
        {
            _context.BudgetChangeRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<List<BudgetChangeRequest>> GetAllAsync()
        {
            return await _context.BudgetChangeRequests.ToListAsync();
        }

        public async Task<BudgetChangeRequest?> GetByIdAsync(int id)
        {
            return await _context.BudgetChangeRequests
                .FirstOrDefaultAsync(r => r.BudgetChangeRequestId == id);
        }
    }
}