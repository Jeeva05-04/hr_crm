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

            public async Task<TrainingResponseDto> AssignTrainingAsync(AssignTrainingDto dto)
            {
                var training = new EmployeeTraining
                {
                    EmployeeId = dto.EmployeeId,
                    TrainingName = dto.TrainingName,
                    Description = dto.Description,
                    IsMandatory = dto.IsMandatory
                };

                var result = await _repo.AddAsync(training);

                return MapToDTO(result);
            }

            public async Task<List<TrainingResponseDto>> GetByEmployeeAsync(int employeeId)
            {
                var list = await _repo.GetByEmployeeIdAsync(employeeId);
                return list.Select(MapToDTO).ToList();
            }
        public async Task<List<TrainingResponseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDTO).ToList();
        }   

        public async Task<bool> UpdateStatusAsync(int id, UpdateTrainingStatusCreateDto dto)
            {
                var training = await _repo.GetByIdAsync(id);
                if (training == null) return false;

                training.Status = dto.Status;

                if (dto.Status == "Completed")
                    training.CompletionDate = DateTime.UtcNow;

                await _repo.UpdateAsync(training);
                return true;
            }

            public async Task<bool> DeleteAsync(int id)
            {
                var training = await _repo.GetByIdAsync(id);
                if (training == null) return false;

                await _repo.DeleteAsync(training);
                return true;
            }

            private TrainingResponseDto MapToDTO(EmployeeTraining t)
            {
                return new TrainingResponseDto
                {
                    Id = t.Id,
                    EmployeeId = t.EmployeeId,
                    TrainingName = t.TrainingName,
                    Description = t.Description,
                    IsMandatory = t.IsMandatory,
                    Status = t.Status,
                    AssignedDate = t.AssignedDate,
                    CompletionDate = t.CompletionDate
                };
            }     
    }
}
        


