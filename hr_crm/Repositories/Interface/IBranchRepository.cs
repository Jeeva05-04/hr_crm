using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IBranchRepository
    {
        Task<List<Branch>> GetAllAsync();
        Task<Branch?> GetByIdAsync(int id);
        Task AddAsync(Branch branch);
        Task<bool> UpdateAsync(int id, Branch branch);
        Task<bool> DeactivateAsync(int id);
    }
}
