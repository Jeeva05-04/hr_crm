using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
            private readonly IPayrollService _service;

            public PayrollController(IPayrollService service)
            {
                _service = service;
            }

            [HttpPost("generate")]
        [HasPermission("Payroll_Generate")]
            public async Task<IActionResult> GeneratePayroll([FromBody] PayrollCreateDto dto)
            {
                var result = await _service.GeneratePayrollAsync(dto);
                return Ok("Payroll generated successfully");
            }
        [HttpGet]
        [HasPermission("Payroll_Getall")]
        public async Task<IActionResult> GetPayroll()
        {
            var data = await _service.GetPayrollAsync();
            return Ok(data);
        }

        [HttpGet("{employeeId}")]
        [HasPermission("Payroll_employeeId")]
            public async Task<IActionResult> GetPayroll(int employeeId)
            {
                var data = await _service.GetPayrollAsync(employeeId);
                return Ok(data);
            }

            [HttpPut("{payrollId}")]
        [HasPermission("payroll_Update")]
            public async Task<IActionResult> UpdatePayroll(int payrollId, [FromBody] PayrollCreateDto dto)
            {
                var result = await _service.UpdatePayrollAsync(payrollId, dto);

                if (!result)
                    return NotFound("Payroll not found");

                return Ok("Payroll updated successfully");
            }

            [HttpDelete("{payrollId}")]
        [HasPermission("Payroll_Delete")]

            public async Task<IActionResult> DeletePayroll(int payrollId)
            {
                var result = await _service.DeletePayrollAsync(payrollId);

                if (!result)
                    return NotFound("Payroll not found");

                return Ok("Payroll deleted successfully");
            }
        
    }
}


