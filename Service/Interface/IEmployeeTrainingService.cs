using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IEmployeeTrainingService
    {
        Task<TrainingResponseDto> AssignTrainingAsync(AssignTrainingDto dto);
        Task<List<TrainingResponseDto>> GetByUserAsync(int userId);
        Task<List<TrainingResponseDto>> GetAllAsync();
        Task<bool> UpdateStatusAsync(int id, UpdateTrainingStatusCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}