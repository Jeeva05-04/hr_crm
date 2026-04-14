using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace hr_crm.Service
{
    public class LearningService : ILearningService
    {
         private readonly ILearningRepository _repo;

            public LearningService(ILearningRepository repo)
            {
                _repo = repo;
            }

            public async Task<LearningCourse> AssignCourse(LearningCourseDto dto)
            {
                var course = new LearningCourse
                {
                    UserId = dto.UserId,
                    CourseName = dto.CourseName,
                    Description = dto.Description,
                    Role = dto.Role,
                    AssignedDate = dto.AssignedDate,
                    DueDate = dto.DueDate,
                    Progress = dto.Progress,
                    Status = "Assigned"
                };

                return await _repo.AssignCourse(course);
            }

            public async Task<List<LearningCourse>> GetUserCourses(int  userId)
            {
                return await _repo.GetUserCourses(userId);
            }

            public async Task UpdateProgress(int id, LearningUpdateProgressDto dto)
            {
                var course = await _repo.GetById(id);
            if (course == null)
                throw new Exception("Course not found");

            course.Progress = dto.Progress;
                course.Status = dto.Status;

                await _repo.Update(course);
            }

            public async Task CompleteCourse(int id)
            {
                var course = await _repo.GetById(id);

                course.Progress = 100;
                course.Status = "Completed";

                await _repo.Update(course);
            }
        public async Task<List<LearningCourse>> GetAllCourses()
        {
            return await _repo.GetAllCourses();
        }
        public async Task DeleteCourse(int id)
        {
            await _repo.Delete(id);
        }

    }
}

