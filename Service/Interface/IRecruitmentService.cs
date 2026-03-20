using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Models;

namespace hr_crm.Service.Interface
{
    public interface IRecruitmentService
    {
        Task<List<Recruitment>> GetAllAsync();
        Task<List<Recruitment>> GetByStatusAsync(string status);
        Task<Recruitment?> GetByIdAsync(int id);
        Task CreateAsync(Recruitment recruitment);
        Task UpdateAsync(Recruitment recruitment);
        Task DeleteAsync(int id);
        Task<(bool Success, string? Error)> ScheduleInterviewAsync(int candidateId, ScheduleInterviewDto dto);
        Task<(bool Success, string? Error)> UpdateStatusAsync(int candidateId, UpdateStatusDto dto);
        Task<(EmployeeOnboarding? Onboarding, string? Error)> ConvertToOnboardingAsync(int candidateId);
        Task<(bool Success, string? Error)> AssignLeadAsync(int candidateId, int assignedToUserId);
        Task<hr_crm.DTO.RecruitmentDashboardDto> GetDashboardAsync();
        Task<hr_crm.DTO.RoleStatsDto?> GetDashboardByRoleAsync(string role);
        Task<hr_crm.DTO.DepartmentStatsDto?> GetDashboardByDepartmentAsync(int departmentId);
    }
}