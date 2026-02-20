using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync();
        Task<bool> CreateAsync(Project project);
        Task<bool> UpdateAsync(int id, Project project);
        Task<bool> DeleteAsync(int id);
    }
}
