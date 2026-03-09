using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IOffBoardingRespository
    {

             
             Task<OffBoarding> CreateAsync(OffBoarding offboarding);
            Task<OffBoarding> GetByIdAsync(int id);
            Task<List<OffBoarding>> GetAllAsync();
            Task<OffBoarding> UpdateAsync(OffBoarding offboarding);
            Task<bool> DeleteAsync(int id);
        
    }
}

