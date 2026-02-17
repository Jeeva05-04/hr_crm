using hr_crm.Entities;
using hr_crm.Repositories;

namespace hr_crm.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repo;

        public DepartmentService(IDepartmentRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Department>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(string name, int branchId)
        {
            var department = new Department
            {
                DepartmentName = name,
                BranchId = branchId
            };

            await _repo.AddAsync(department);
            return true;
        }

        public Task<bool> UpdateAsync(int id, string name, int branchId)
        {
            var department = new Department
            {
                DepartmentName = name,
                BranchId = branchId
            };

            return _repo.UpdateAsync(id, department);
        }

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}
