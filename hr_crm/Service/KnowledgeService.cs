using hr_crm.Entities;
using hr_crm.Repositories;

namespace hr_crm.Services
{
    public class KnowledgeService : IKnowledgeService
    {
        private readonly IKnowledgeRepository _repo;

        public KnowledgeService(IKnowledgeRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Knowledge>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task CreateAsync(Knowledge knowledge)
            => _repo.AddAsync(knowledge);

        public Task<bool> DeactivateAsync(int id)
            => _repo.DeactivateAsync(id);
    }
}
