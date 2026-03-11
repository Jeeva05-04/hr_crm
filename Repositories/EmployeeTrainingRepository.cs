using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
namespace hr_crm.Repositories
{
    public class EmployeeTrainingRepository : IEmployeeTrainingRepository
    {
         
            private readonly AppDbContext _context;

            public EmployeeTrainingRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<EmployeeTraining> AddAsync(EmployeeTraining training)
            {
                _context.EmployeeTrainings.Add(training);
                await _context.SaveChangesAsync();
                return training;
            }

            public async Task<List<EmployeeTraining>> GetByUserIdAsync(int userId)
            {
                return await _context.EmployeeTrainings
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
            }

            public async Task<EmployeeTraining> GetByIdAsync(int id)
            {
                return await _context.EmployeeTrainings.FindAsync(id);
            }
        public async Task<List<EmployeeTraining>> GetAllAsync()
        {
            return await _context.EmployeeTrainings.ToListAsync();
        }

        public async Task UpdateAsync(EmployeeTraining training)
            {
                _context.EmployeeTrainings.Update(training);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(EmployeeTraining training)
            {
                _context.EmployeeTrainings.Remove(training);
                await _context.SaveChangesAsync();
            }
        
    }

}


