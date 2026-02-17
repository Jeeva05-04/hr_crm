using hr_crm.Entities;

namespace hr_crm.Services
{
    public interface IKnowledgeService
    {
        Task<List<Knowledge>> GetAllAsync();
        Task CreateAsync(Knowledge knowledge);
        Task<bool> DeactivateAsync(int id);
    }
}
