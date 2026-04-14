using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IJobOpeningService
    {
        Task<List<JobOpeningResponseDto>> GetAllAsync();
        Task<List<JobOpeningResponseDto>> GetByDepartmentAsync(int departmentId);
        Task<JobOpeningResponseDto?> GetByIdAsync(int jobOpeningId);
        Task<JobOpeningResponseDto> CreateAsync(JobOpeningCreateDto dto);
        Task<(bool Success, string? Error)> UpdateAsync(int jobOpeningId, JobOpeningUpdateDto dto);
        Task<(bool Success, string? Error)> DeleteAsync(int jobOpeningId);
    }
}
