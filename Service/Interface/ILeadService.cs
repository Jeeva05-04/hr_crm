using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface ILeadService
    {
        Task<List<LeadResponseDto>> GetAllAsync();
        Task<List<LeadResponseDto>> GetByStatusAsync(string status);
        Task<List<LeadResponseDto>> GetByAssignedUserAsync(int userId);
        Task<LeadResponseDto?> GetByIdAsync(int leadId);
        Task<LeadResponseDto> CreateAsync(LeadCreateDto dto);
        Task<(bool Success, string? Error)> AssignLeadAsync(int leadId, LeadAssignDto dto);
        Task<(bool Success, string? Error)> UpdateStatusAsync(int leadId, LeadUpdateStatusDto dto);
        Task<(bool Success, string? Error)> DeleteAsync(int leadId);
    }
}
