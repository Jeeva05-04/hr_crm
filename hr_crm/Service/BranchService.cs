using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _repo;

        public BranchService(IBranchRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Branch>> GetAllAsync()
            => _repo.GetAllAsync();

        public async Task<bool> CreateAsync(string name, string location, string status)
        {
            var branch = new Branch
            {
                BranchName = name,
                Location = location,
                Status = status
            };

            await _repo.AddAsync(branch);
            return true;
        }

        public Task<bool> UpdateAsync(int id, string name, string location, string status)
        {
            var branch = new Branch
            {
                BranchName = name,
                Location = location,
                Status = status
            };

            return _repo.UpdateAsync(id, branch);
        }

        public Task<bool> DeactivateAsync(int id)
            => _repo.DeactivateAsync(id);
    }
}
