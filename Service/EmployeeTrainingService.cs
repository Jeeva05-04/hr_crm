using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class EmployeeTrainingService : IEmployeeTrainingService
    {
        private readonly IEmployeeTrainingRepository _repo;

        public EmployeeTrainingService(IEmployeeTrainingRepository repo)
        {
            _repo = repo;
        }

        // Assign training
        public async Task<TrainingResponseDto> AssignTrainingAsync(AssignTrainingDto dto)
        {
            var training = new EmployeeTraining
            {
                UserId = dto.UserId,
                TrainingName = dto.TrainingName,
                Description = dto.Description,
                IsMandatory = dto.IsMandatory,

                TrainingProvider = dto.TrainingProvider,
                Category = dto.Category,
                DurationHours = dto.DurationHours,
                AssignedBy = dto.AssignedBy,
                DueDate = dto.DueDate,

                Status = "Assigned",
                Progress = 0,
                AssignedDate = DateTime.UtcNow,
                CompletionDate = null,
                IsCertified = false,
                Feedback = null,
                Score = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var result = await _repo.AddAsync(training);
            return MapToDTO(result);
        }

        // Get trainings by user
        public async Task<List<TrainingResponseDto>> GetByUserAsync(int userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);
            return list.Select(MapToDTO).ToList();
        }

        // Get all trainings
        public async Task<List<TrainingResponseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDTO).ToList();
        }

        // Update training status
        public async Task<bool> UpdateStatusAsync(int id, UpdateTrainingStatusCreateDto dto)
        {
            var training = await _repo.GetByIdAsync(id);

            if (training == null)
                return false;

            training.Status = dto.Status;
            training.Progress = dto.Progress;
            training.IsCertified = dto.IsCertified;
            training.Score = dto.Score;
            training.Feedback = dto.Feedback;
            training.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "Completed")
            {
                training.CompletionDate = dto.CompletionDate ?? DateTime.UtcNow;
                training.Progress = 100;
            }

            await _repo.UpdateAsync(training);
            return true;
        }

        // Delete training
        public async Task<bool> DeleteAsync(int id)
        {
            var training = await _repo.GetByIdAsync(id);

            if (training == null)
                return false;

            await _repo.DeleteAsync(training);
            return true;
        }

        // Mapping Entity -> DTO
        private TrainingResponseDto MapToDTO(EmployeeTraining t)
        {
            return new TrainingResponseDto
            {
                Id = t.Id,
                UserId = t.UserId,
                TrainingName = t.TrainingName,
                Description = t.Description,
                IsMandatory = t.IsMandatory,
                Status = t.Status,
                Progress = t.Progress,
                AssignedDate = t.AssignedDate,
                DueDate = t.DueDate,
                CompletionDate = t.CompletionDate,
                TrainingProvider = t.TrainingProvider,
                Category = t.Category,
                DurationHours = t.DurationHours,
                AssignedBy = t.AssignedBy,
                IsCertified = t.IsCertified,
                Feedback = t.Feedback,
                Score = t.Score,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
        }
    }
}