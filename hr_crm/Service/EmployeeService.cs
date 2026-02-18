using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Employee>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(Employee employee)
        {
            await _repo.AddAsync(employee);
            return true;
        }

        public Task<bool> UpdateAsync(int id, Employee employee)
            => _repo.UpdateAsync(id, employee);

        public Task<bool> DeactivateAsync(int id)
            => _repo.DeactivateAsync(id);
    }
}
