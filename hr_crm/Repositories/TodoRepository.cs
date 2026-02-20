using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _context;

        public TodoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TodoTask>> GetAllAsync()
        {
            return await _context.TodoTasks
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TodoTask?> GetByIdAsync(int id)
        {
            return await _context.TodoTasks
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async Task AddAsync(TodoTask task)
        {
            task.CreatedAt = DateTime.UtcNow;
            _context.TodoTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, TodoTask task)
        {
            var existing = await _context.TodoTasks.FindAsync(id);
            if (existing == null)
                return false;

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.AssignedTo = task.AssignedTo;
            existing.DueDate = task.DueDate;
            existing.Status = task.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.TodoTasks.FindAsync(id);
            if (task == null)
                return false;

            _context.TodoTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
