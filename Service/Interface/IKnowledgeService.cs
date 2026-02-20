using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IKnowledgeService
    {
        Task<List<Knowledge>> GetAllAsync();
        Task CreateAsync(Knowledge knowledge);
        Task<bool> DeactivateAsync(int id);
    }
}
