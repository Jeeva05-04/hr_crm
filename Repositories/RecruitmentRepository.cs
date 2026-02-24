using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
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

        // =========================================
        // ✅ GET ALL
        // =========================================
        public async Task<List<Recruitment>> GetAllAsync()
        {
            return await _context.Recruitments
                .OrderByDescending(r => r.ApplicationDate)
                .ToListAsync();
        }

        // =========================================
        // ✅ GET BY ID
        // =========================================
        public async Task<Recruitment?> GetByIdAsync(int id)
        {
            return await _context.Recruitments
                .FirstOrDefaultAsync(r => r.CandidateId == id);
        }

        // =========================================
        // ✅ CREATE
        // =========================================
        public async Task AddAsync(Recruitment recruitment)
        {
            _context.Recruitments.Add(recruitment);
            await _context.SaveChangesAsync();
        }

        // =========================================
        // ✅ UPDATE
        // =========================================
        public async Task UpdateAsync(Recruitment recruitment)
        {
            _context.Recruitments.Update(recruitment);
            await _context.SaveChangesAsync();
        }

        // =========================================
        // ✅ DELETE
        // =========================================
        public async Task DeleteAsync(int id)
        {
            var candidate = await _context.Recruitments
                .FirstOrDefaultAsync(r => r.CandidateId == id);

            if (candidate != null)
            {
                _context.Recruitments.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }
    }
}