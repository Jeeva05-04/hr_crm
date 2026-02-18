using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IRecruitmentService
    {
        Task<List<Recruitment>> GetAllAsync();
        Task CreateAsync(Recruitment recruitment);
    }
}
