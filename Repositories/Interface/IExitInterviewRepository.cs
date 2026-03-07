using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories.Interface
{
  
    public interface IExitInterviewRepository
    {
        Task<ExitInterview> ScheduleExitInterview(ExitInterview interview);

        Task<ExitInterview> SubmitFeedback(ExitInterview interview);

        Task<ExitInterview> GetByEmployeeId(int employeeId);

        Task<List<ExitInterview>> GetAll();
        Task<ExitInterview> GetById(int id);

        Task<ExitInterview> Update(ExitInterview interview);

        Task Delete(int id);
      
    }
}