using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.Department)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task AddAsync(Project project)
        {
            project.CreatedDate = DateTime.UtcNow;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, Project project)
        {
            var existing = await _context.Projects.FindAsync(id);
            if (existing == null)
                return false;

            existing.ProjectName = project.ProjectName;
            existing.Duration = project.Duration;
            existing.Status = project.Status;
            existing.ManagerId = project.ManagerId;
            existing.DepartmentId = project.DepartmentId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
