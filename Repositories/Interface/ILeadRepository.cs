using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface ILeadRepository
    {
        Task<List<Lead>> GetAllAsync();
        Task<List<Lead>> GetByStatusAsync(string status);
        Task<List<Lead>> GetByAssignedUserAsync(int userId);
        Task<Lead?> GetByIdAsync(int leadId);
        Task<Lead> AddAsync(Lead lead);
        Task<Lead> UpdateAsync(Lead lead);
        Task DeleteAsync(int leadId);
    }
}
