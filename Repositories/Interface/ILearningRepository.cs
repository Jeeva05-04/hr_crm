using hr_crm.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hr_crm.Repositories.Interface
{
    public interface ILearningRepository
    {

        Task<LearningCourse> AssignCourse(LearningCourse course);

        Task<List<LearningCourse>> GetEmployeeCourses(int employeeId);

        Task<LearningCourse?> GetById(int id);

        Task Update(LearningCourse course);
        Task Delete(int id);
        Task<List<LearningCourse>> GetAllCourses();

    }
}
