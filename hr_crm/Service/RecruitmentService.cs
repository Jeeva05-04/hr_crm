using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly IRecruitmentRepository _repo;

        public RecruitmentService(IRecruitmentRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Recruitment>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task CreateAsync(Recruitment recruitment)
            => _repo.AddAsync(recruitment);
    }
}
