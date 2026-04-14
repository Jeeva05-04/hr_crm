using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface ILearningService
    {
        Task<LearningCourse> AssignCourse(LearningCourseDto dto);
        Task<List<LearningCourse>> GetUserCourses(int userId);
        Task UpdateProgress(int id, LearningUpdateProgressDto dto);
        Task CompleteCourse(int id);
        Task<List<LearningCourse>> GetAllCourses();
        Task DeleteCourse(int id);
    }
}