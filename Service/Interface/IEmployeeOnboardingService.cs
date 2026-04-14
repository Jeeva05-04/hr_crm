using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IEmployeeOnboardingService
    {
        Task<EmployeeOnboarding> CreateAsync(EmployeeOnboardingCreateDto dto, string webRootPath);

        Task<List<DTO.EmployeeOnboardingResponseDto>> GetAllAsync();

        Task<DTO.EmployeeOnboardingResponseDto?> GetByIdAsync(int id);

        Task<bool> DeleteAsync(int id);

        Task<EmployeeOnboardingDocuments?> GetDocumentsAsync(int onboardingId);
        Task<WorkExperience?> GetWorkExperienceAsync(int onboardingId);
        Task<(AuthUser? User, string? Error)> ConvertToUserAsync(int onboardingId, int actingUserId);

        // Invite link methods
        Task<OnboardingInvite> GenerateInviteAsync(GenerateInviteDto dto, int createdByUserId, string baseUrl);
        Task<(bool Valid, string? Error, OnboardingInvite? Invite)> ValidateTokenAsync(string token);
        Task<(EmployeeOnboarding? Record, string? Error)> SubmitWithTokenAsync(string token, EmployeeOnboardingCreateDto dto, string webRootPath);
        Task<List<OnboardingInvite>> GetAllInvitesAsync();
    }
}
