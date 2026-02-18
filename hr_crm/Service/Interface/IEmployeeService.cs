using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<bool> CreateAsync(Employee employee);
        Task<bool> UpdateAsync(int id, Employee employee);
        Task<bool> DeactivateAsync(int id);
    }
}
