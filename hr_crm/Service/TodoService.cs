using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repo;

        public TodoService(ITodoRepository repo)
        {
            _repo = repo;
        }

        public Task<List<TodoTask>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(TodoTask task)
        {
            await _repo.AddAsync(task);
            return true;
        }

        public Task<bool> UpdateAsync(int id, TodoTask task)
            => _repo.UpdateAsync(id, task);

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
