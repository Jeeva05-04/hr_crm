using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IOffBoardingService
    {
            Task<OffBoarding> CreateOffboarding(OffBoardingDto dto);
            Task<OffBoarding> GetOffboarding(int id);
            Task<List<OffBoarding>> GetAllOffboardings();
            Task<OffBoarding> UpdateStatus(int id, UpdateOffboardingStatusDTO dto);
            Task<bool> DeleteOffboarding(int id);
        
    }
}

