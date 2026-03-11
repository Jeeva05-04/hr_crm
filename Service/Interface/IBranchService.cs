using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IBranchService
    {
        Task<List<Branch>> GetAllAsync();
        Task<bool> CreateAsync(string name, string location, string status);
        Task<bool> UpdateAsync(int id, string name, string location, string status);
        Task<bool> DeleteAsync(int id);
    }
}
