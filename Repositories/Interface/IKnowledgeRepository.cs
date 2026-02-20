using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IKnowledgeRepository
    {
        Task<List<Knowledge>> GetAllAsync();
        Task AddAsync(Knowledge knowledge);
        Task<bool> DeactivateAsync(int id);
    }
}
