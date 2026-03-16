using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repo;
        private readonly NotificationService _notification;

        public TodoService(ITodoRepository repo, NotificationService notification)
        {
            _repo = repo;
            _notification = notification;
        }

        public Task<List<TodoTask>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(TodoTask task, string assignerName = "HR Manager")
        {
            await _repo.AddAsync(task);

            await _notification.CreateNotification(
                task.AssignedTo,
                $"New Task Assigned: {task.Title}",
                $"Task: {task.Title}\nDescription: {task.Description}\nDeadline: {task.DueDate:dd MMM yyyy}\nAssigned by: {assignerName}",
                "Todo",
                task.TaskId
            );

            return true;
        }

        public Task<bool> UpdateAsync(int id, TodoTask task)
            => _repo.UpdateAsync(id, task);

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
