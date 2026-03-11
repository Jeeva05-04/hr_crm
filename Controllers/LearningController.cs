using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LearningController : ControllerBase
    {
            private readonly ILearningService _service;

            public LearningController(ILearningService service)
            {
                _service = service;
            }

            [HttpPost("assign-course")]
            [HasPermission("LEARNING_ASSIGN")]
            public async Task<IActionResult> AssignCourse(LearningCourseDto dto)
            {
                var result = await _service.AssignCourse(dto);
                return Ok(result);
            }

            [HttpGet("user/{userId}")]
            [HasPermission("LEARNING_VIEW")]
            public async Task<IActionResult> GetUserCourses(int userId)
            {
                var result = await _service.GetUserCourses(userId);
                return Ok(result);
            }

            [HttpPut("update-progress/{id}")]
                [HasPermission("LEARNING_UPDATE")]
        public async Task<IActionResult> UpdateProgress(int id, LearningUpdateProgressDto dto)
            {
                await _service.UpdateProgress(id, dto);
                return Ok("Progress Updated");
            }

            [HttpPut("complete/{id}")]
            [HasPermission("LEARNING_UPDATE")]
        public async Task<IActionResult> CompleteCourse(int id)
            {
                await _service.CompleteCourse(id);
                return Ok("Course Completed");
            }
        [HttpGet("all")]
            [HasPermission("LEARNING_VIEW")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _service.GetAllCourses();
            return Ok(courses);
        }
        [HttpDelete("delete/{id}")]
        [HasPermission("LEARNING_DELETE")]
        public async Task<IActionResult> DeleteCourse(int id)
            {
                await _service.DeleteCourse(id);
           
                return Ok("Course deleted successfully");
             }

    }
}

