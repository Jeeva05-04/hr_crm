using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using hr_crm.Service;
using hr_crm.Authorization;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffBoardingController : ControllerBase
    {
            private readonly IOffBoardingService _service;

            public OffBoardingController(IOffBoardingService service)
            {
                _service = service;
            }

            [HttpPost("create")]
            [HasPermission("OffBoarding_Create")]
            public async Task<IActionResult> Create(OffBoardingDto dto)
            {
                var result = await _service.CreateOffboarding(dto);
                return Ok(result);
            }

            [HttpGet("{id}")]
                [HasPermission("OffBoarding_Get/{id}")]
        public async Task<IActionResult> Get(int id)
            {
                var result = await _service.GetOffboarding(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
             [HttpGet]
                [HasPermission("OffBoarding_View")]  
        public async Task<IActionResult> GetAll()
             {
                 var result = await _service.GetAllOffboardings();
                  return Ok(result);
               }

              [HttpPut("update-status/{id}")]
                [HasPermission("OffBoarding_Update")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOffboardingStatusDTO dto)
            {
                var result = await _service.UpdateStatus(id, dto);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            [HttpDelete("delete/{id}")]
            [HasPermission("OffBoarding_Delete")]
        public async Task<IActionResult> Delete(int id)
            {
                var result = await _service.DeleteOffboarding(id);
                if (!result)
                    return NotFound();

                return Ok("Deleted Successfully");
            }
        
    }
}

