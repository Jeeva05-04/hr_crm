using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class KnowledgeRepository : IKnowledgeRepository
    {
        private readonly AppDbContext _context;

        public KnowledgeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Knowledge>> GetAllAsync()
        {
            return await _context.Knowledges
                .OrderByDescending(k => k.CreatedDate)
                .ToListAsync();
        }

        public async Task AddAsync(Knowledge knowledge)
        {
            _context.Knowledges.Add(knowledge);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var record = await _context.Knowledges
                .FirstOrDefaultAsync(k => k.BranchId == id); // adjust if you add KnowledgeId later

            if (record == null)
                return false;

            record.Status = "Inactive";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
