using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class JobOpeningRepository : IJobOpeningRepository
    {
        private readonly AppDbContext _context;

        public JobOpeningRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<JobOpening>> GetAllAsync()
            => await _context.JobOpenings
                .Include(j => j.Department)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<List<JobOpening>> GetByDepartmentAsync(int departmentId)
            => await _context.JobOpenings
                .Include(j => j.Department)
                .Where(j => j.DepartmentId == departmentId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<JobOpening?> GetByIdAsync(int jobOpeningId)
            => await _context.JobOpenings
                .Include(j => j.Department)
                .FirstOrDefaultAsync(j => j.JobOpeningId == jobOpeningId);

        public async Task<JobOpening> AddAsync(JobOpening jobOpening)
        {
            _context.JobOpenings.Add(jobOpening);
            await _context.SaveChangesAsync();
            return jobOpening;
        }

        public async Task<JobOpening> UpdateAsync(JobOpening jobOpening)
        {
            _context.JobOpenings.Update(jobOpening);
            await _context.SaveChangesAsync();
            return jobOpening;
        }

        public async Task DeleteAsync(int jobOpeningId)
        {
            var job = await _context.JobOpenings.FindAsync(jobOpeningId);
            if (job != null)
            {
                _context.JobOpenings.Remove(job);
                await _context.SaveChangesAsync();
            }
        }
    }
}
