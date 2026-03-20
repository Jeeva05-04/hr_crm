using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class JobOpeningService : IJobOpeningService
    {
        private readonly IJobOpeningRepository _repo;
        private readonly AppDbContext _context;

        public JobOpeningService(IJobOpeningRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<List<JobOpeningResponseDto>> GetAllAsync()
        {
            var jobs = await _repo.GetAllAsync();
            return await MapListWithFilledCount(jobs);
        }

        public async Task<List<JobOpeningResponseDto>> GetByDepartmentAsync(int departmentId)
        {
            var jobs = await _repo.GetByDepartmentAsync(departmentId);
            return await MapListWithFilledCount(jobs);
        }

        public async Task<JobOpeningResponseDto?> GetByIdAsync(int jobOpeningId)
        {
            var job = await _repo.GetByIdAsync(jobOpeningId);
            if (job is null) return null;
            return await MapWithFilledCount(job);
        }

        public async Task<JobOpeningResponseDto> CreateAsync(JobOpeningCreateDto dto)
        {
            var job = new JobOpening
            {
                Title = dto.Title,
                DepartmentId = dto.DepartmentId,
                TotalOpenings = dto.TotalOpenings,
                Description = dto.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };
            var created = await _repo.AddAsync(job);
            // Reload with Department included
            var full = await _repo.GetByIdAsync(created.JobOpeningId);
            return await MapWithFilledCount(full!);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int jobOpeningId, JobOpeningUpdateDto dto)
        {
            var job = await _repo.GetByIdAsync(jobOpeningId);
            if (job is null) return (false, "Job opening not found.");

            if (dto.Title != null) job.Title = dto.Title;
            if (dto.TotalOpenings.HasValue) job.TotalOpenings = dto.TotalOpenings.Value;
            if (dto.Description != null) job.Description = dto.Description;
            if (dto.Status != null)
            {
                var validStatuses = new[] { "Open", "Closed", "Paused" };
                if (!validStatuses.Contains(dto.Status))
                    return (false, $"Invalid status. Allowed: {string.Join(", ", validStatuses)}");

                job.Status = dto.Status;
                if (dto.Status == "Closed") job.ClosedAt = DateTime.UtcNow;
            }

            await _repo.UpdateAsync(job);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int jobOpeningId)
        {
            var job = await _repo.GetByIdAsync(jobOpeningId);
            if (job is null) return (false, "Job opening not found.");

            await _repo.DeleteAsync(jobOpeningId);
            return (true, null);
        }

        // Count how many candidates have been hired (Onboarded) for this job title
        private async Task<int> GetFilledCount(string title, int departmentId)
            => await _context.Recruitments
                .CountAsync(r => r.AppliedPosition == title
                              && r.DepartmentId == departmentId
                              && r.Status == "Onboarded");

        private async Task<JobOpeningResponseDto> MapWithFilledCount(JobOpening job)
        {
            var filled = await GetFilledCount(job.Title, job.DepartmentId);
            return new JobOpeningResponseDto
            {
                JobOpeningId = job.JobOpeningId,
                Title = job.Title,
                DepartmentId = job.DepartmentId,
                DepartmentName = job.Department?.DepartmentName ?? string.Empty,
                TotalOpenings = job.TotalOpenings,
                Description = job.Description,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                ClosedAt = job.ClosedAt,
                FilledCount = filled,
                RemainingOpenings = Math.Max(0, job.TotalOpenings - filled)
            };
        }

        private async Task<List<JobOpeningResponseDto>> MapListWithFilledCount(List<JobOpening> jobs)
        {
            var result = new List<JobOpeningResponseDto>();
            foreach (var job in jobs)
                result.Add(await MapWithFilledCount(job));
            return result;
        }
    }
}
