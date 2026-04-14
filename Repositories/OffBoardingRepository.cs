using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class OffBoardingRepository : IOffBoardingRespository
    {
        private readonly AppDbContext _context;

        public OffBoardingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OffBoarding> CreateAsync(OffBoarding offboarding)
        {
            _context.OffBoardings.Add(offboarding);
            await _context.SaveChangesAsync();
            return offboarding;
        }
        public async Task<List<OffBoarding>> GetAllAsync()
        {
            return await _context.OffBoardings.ToListAsync();
        }

        public async Task<OffBoarding> GetByIdAsync(int id)
        {
            return await _context.OffBoardings.FindAsync(id);
        }

        public async Task<OffBoarding> UpdateAsync(OffBoarding offboarding)
        {
            _context.OffBoardings.Update(offboarding);
            await _context.SaveChangesAsync();
            return offboarding;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _context.OffBoardings.FindAsync(id);
            if (record == null)
                return false;

            _context.OffBoardings.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


