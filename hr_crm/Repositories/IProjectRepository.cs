using hr_crm.Entities;

namespace hr_crm.Repositories
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task AddAsync(Project project);
        Task<bool> UpdateAsync(int id, Project project);
        Task<bool> DeleteAsync(int id);
    }
}
