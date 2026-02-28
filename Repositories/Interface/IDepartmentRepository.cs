using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task AddAsync(Department department);
        Task<bool> UpdateAsync(int id, Department department);
        Task<bool> DeleteAsync(int id);
        Task<object?> GetUsersInDepartmentAsync(int departmentId);
    }
}
