using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly AppDbContext _context;

        public BranchRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Branch>> GetAllAsync()
        {
            return await _context.Branches
                .OrderBy(b => b.BranchName)
                .ToListAsync();
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Branches
                .FirstOrDefaultAsync(b => b.BranchId == id);
        }

        public async Task AddAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, Branch branch)
        {
            var existing = await _context.Branches.FindAsync(id);
            if (existing == null)
                return false;

            existing.BranchName = branch.BranchName;
            existing.Location = branch.Location;
            existing.Status = branch.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return false;

            branch.Status = "Inactive";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
