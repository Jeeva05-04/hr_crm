
using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class LearningRespository : ILearningRepository
    {
        private readonly AppDbContext _context;

            public LearningRespository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<LearningCourse> AssignCourse(LearningCourse course)
            {
                _context.LearningCourses.Add(course);
                await _context.SaveChangesAsync();
                return course;
            }

            public async Task<List<LearningCourse>> GetUserCourses(int userId)
            {
                return await _context.LearningCourses
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
            }

            public async Task<LearningCourse?> GetById(int id)
            {
                return await _context.LearningCourses.FindAsync(id);
            }

            public async Task Update(LearningCourse course)
            {
                _context.LearningCourses.Update(course);
                await _context.SaveChangesAsync();
            }
        public async Task<List<LearningCourse>> GetAllCourses()
        {
            return await _context.LearningCourses.ToListAsync();
        }
        public async Task Delete(int id)
        {
            var course = await _context.LearningCourses.FindAsync(id);

            if (course != null)
            {
                _context.LearningCourses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

    }
}


