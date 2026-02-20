using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface ITodoService
    {
        Task<List<TodoTask>> GetAllAsync();
        Task<bool> CreateAsync(TodoTask task);
        Task<bool> UpdateAsync(int id, TodoTask task);
        Task<bool> DeleteAsync(int id);
    }
}
