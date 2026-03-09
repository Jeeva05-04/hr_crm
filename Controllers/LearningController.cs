using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;


namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LearningController : ControllerBase
    {
            private readonly ILearningService _service;

            public LearningController(ILearningService service)
            {
                _service = service;
            }

            [HttpPost("assign-course")]
            [HasPermission("Learning_assign-course")]
            public async Task<IActionResult> AssignCourse(LearningCourseDto dto)
            {
                var result = await _service.AssignCourse(dto);
                return Ok(result);
            }

            [HttpGet("employee/{employeeId}")]
            [HasPermission("Learning_employee/{employeeId}")]
            public async Task<IActionResult> GetEmployeeCourses(int employeeId)
            {
                var result = await _service.GetEmployeeCourses(employeeId);
                return Ok(result);
            }

            [HttpPut("update-progress/{id}")]
                [HasPermission("Learning_update-progress/{id}")]
        public async Task<IActionResult> UpdateProgress(int id, LearningUpdateProgressDto dto)
            {
                await _service.UpdateProgress(id, dto);
                return Ok("Progress Updated");
            }

            [HttpPut("complete/{id}")]
            [HasPermission("Learning_complete/{id}")]
        public async Task<IActionResult> CompleteCourse(int id)
            {
                await _service.CompleteCourse(id);
                return Ok("Course Completed");
            }
        [HttpGet("all")]
            [HasPermission("Learning_view")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _service.GetAllCourses();
            return Ok(courses);
        }
        [HttpDelete("delete/{id}")]
        [HasPermission("Learning_Delete")]
        public async Task<IActionResult> DeleteCourse(int id)
            {
                await _service.DeleteCourse(id);
           
                return Ok("Course deleted successfully");
             }

    }
}

