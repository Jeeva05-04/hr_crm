using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface ITodoRepository
    {
        Task<List<TodoTask>> GetAllAsync();
        Task<TodoTask?> GetByIdAsync(int id);
        Task AddAsync(TodoTask task);
        Task<bool> UpdateAsync(int id, TodoTask task);
        Task<bool> DeleteAsync(int id);
    }
}
