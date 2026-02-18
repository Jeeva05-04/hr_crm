using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync();
        Task<bool> CreateAsync(string name, int branchId);
        Task<bool> UpdateAsync(int id, string name, int branchId);
        Task<bool> DeleteAsync(int id);
    }
}
