using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;


namespace hr_crm.Service
{
    public class OffBoardingService : IOffBoardingService
    {
            private readonly IOffBoardingRespository _repo;

            public OffBoardingService(IOffBoardingRespository repo)
            {
            _repo = repo;
            }

            public async Task<OffBoarding> CreateOffboarding(OffBoardingDto dto)
            {
                var offboarding = new OffBoarding
                {
                    EmployeeId = dto.EmployeeId,
                    ResignationDate = dto.ResignationDate,
                    LastWorkingDate = dto.LastWorkingDate,
                    Reason = dto.Reason,
                    KnowledgeTransferStatus = "Pending",
                    AssetReturnStatus = "Pending",
                    ExitInterviewStatus = "Pending",
                    OverallStatus = "Initiated",
                    AccountDeactivation = dto.AccountDeactivation
                };

                return await _repo.CreateAsync(offboarding);
            }

            public async Task<OffBoarding> GetOffboarding(int id)
            {
                return await _repo.GetByIdAsync(id);
            }
        public async Task<List<OffBoarding>> GetAllOffboardings()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<OffBoarding> UpdateStatus(int id, UpdateOffboardingStatusDTO dto)
            {
                var record = await _repo.GetByIdAsync(id);
                if (record == null)
                    return null;

                record.KnowledgeTransferStatus = dto.KnowledgeTransferStatus;
                record.AssetReturnStatus = dto.AssetReturnStatus;
                record.ExitInterviewStatus = dto.ExitInterviewStatus;
                record.OverallStatus = dto.OverallStatus;

                return await _repo.UpdateAsync(record);
            }

            public async Task<bool> DeleteOffboarding(int id)
            {
                return await _repo.DeleteAsync(id);
            }     
    }
}

