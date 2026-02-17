using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class RecruitmentRepository : IRecruitmentRepository
    {
        private readonly AppDbContext _context;

        public RecruitmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Recruitment>> GetAllAsync()
        {
            return await _context.Recruitments
                .OrderByDescending(r => r.ApplicationDate)
                .ToListAsync();
        }

        public async Task AddAsync(Recruitment recruitment)
        {
            _context.Recruitments.Add(recruitment);
            await _context.SaveChangesAsync();
        }
    }
}
