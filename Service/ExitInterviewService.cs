using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class ExitInterviewService : IExitInterviewService
    {
        private readonly IExitInterviewRepository _repo;

        public ExitInterviewService(IExitInterviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<ExitInterviewResponseDto> ScheduleInterview(ExitInterviewRequestDto dto)
        {
            var interview = new ExitInterview
            {
                UserId = dto.UserId,
                ScheduledDate = dto.ScheduledDate,
                Status = "Scheduled"
            };

            var result = await _repo.ScheduleExitInterview(interview);

            return MapToResponse(result);
        }

        public async Task<ExitInterviewResponseDto> SubmitFeedback(ExitInterviewFeedbackDto dto)
        {
            var interview = await _repo.GetByUserId(dto.UserId);

            if (interview == null)
                throw new Exception("Exit interview not found");

            interview.ReasonForLeaving = dto.ReasonForLeaving;
            interview.Feedback = dto.Feedback;
            interview.Suggestions = dto.Suggestions;
            interview.Status = "Completed";

            var result = await _repo.SubmitFeedback(interview);

            return MapToResponse(result);
        }

        public async Task<ExitInterviewResponseDto> GetByUserId(int userId)
        {
            var interview = await _repo.GetByUserId(userId);

            if (interview == null)
                throw new Exception("Exit interview not found");

            return MapToResponse(interview);
        }

        public async Task<List<ExitInterviewResponseDto>> GetAll()
        {
            var interviews = await _repo.GetAll();

            return interviews.Select(MapToResponse).ToList();
        }

        private ExitInterviewResponseDto MapToResponse(ExitInterview interview)
        {
            return new ExitInterviewResponseDto
            {
                Id = interview.Id,
                UserId = interview.UserId,
                ScheduledDate = interview.ScheduledDate,
                ReasonForLeaving = interview.ReasonForLeaving,
                Feedback = interview.Feedback,
                Suggestions = interview.Suggestions,
                Status = interview.Status
            };
        }

        public async Task<ExitInterviewResponseDto> UpdateInterview(int id, ExitInterviewResponseDto dto)
        {
            var interview = await _repo.GetById(id);

            if (interview == null)
                throw new Exception("Exit interview not found");

            interview.UserId = dto.UserId;
            interview.ScheduledDate = dto.ScheduledDate;
            interview.ReasonForLeaving = dto.ReasonForLeaving;
            interview.Feedback = dto.Feedback;
            interview.Suggestions = dto.Suggestions;
            interview.Status = dto.Status;

            var result = await _repo.Update(interview);

            return MapToResponse(result);
        }

        public async Task DeleteInterview(int id)
        {
            await _repo.Delete(id);
        }
    }
}