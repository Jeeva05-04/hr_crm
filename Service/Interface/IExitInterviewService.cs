
using hr_crm.DTO;


namespace hr_crm.Service.Interface
{
    public interface IExitInterviewService
    {
        Task<ExitInterviewResponseDto> ScheduleInterview(ExitInterviewRequestDto dto);

        Task<ExitInterviewResponseDto> SubmitFeedback(ExitInterviewFeedbackDto dto);

        Task<ExitInterviewResponseDto> GetByEmployeeId(int employeeId);

        Task<List<ExitInterviewResponseDto>> GetAll();
        Task<ExitInterviewResponseDto> UpdateInterview(int id, ExitInterviewResponseDto dto);
        Task DeleteInterview(int id);
    }
}
