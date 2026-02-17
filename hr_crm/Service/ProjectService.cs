using hr_crm.Entities;
using hr_crm.Repositories;

namespace hr_crm.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;

        public ProjectService(IProjectRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Project>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(Project project)
        {
            await _repo.AddAsync(project);
            return true;
        }

        public Task<bool> UpdateAsync(int id, Project project)
            => _repo.UpdateAsync(id, project);

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
