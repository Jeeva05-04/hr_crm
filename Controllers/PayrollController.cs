using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        [HasPermission("PAYROLL_GENERATE")]
        public async Task<IActionResult> GeneratePayroll([FromBody] PayrollCreateDto dto)
        {
            var result = await _service.GeneratePayrollAsync(dto);
            return Ok(result);
        }

        [HttpGet("all")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetPayroll()
        {
            var result = await _service.GetAllPayrollAsync();
            return Ok(result);
        }

        [HttpGet("{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetPayrollByUserId(int userId)
        {
            var result = await _service.GetPayrollByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpPut("{payrollId}")]
        [HasPermission("PAYROLL_UPDATE")]
        public async Task<IActionResult> UpdatePayroll(int payrollId, [FromBody] PayrollCreateDto dto)
        {
            var result = await _service.UpdatePayrollAsync(payrollId, dto);

            if (result == null)
                return NotFound("Payroll record not found");

            return Ok(result);
        }
        [HttpDelete("{payrollId}")]
        [HasPermission("PAYROLL_DELETE")]
        public async Task<IActionResult> DeletePayroll(int payrollId)
        {
            var result = await _service.DeletePayrollAsync(payrollId);

            if (!result)
                return NotFound("Payroll record not found");

            return Ok("Payroll deleted successfully");
        }

        [HttpPost("allowance")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> AddAllowance([FromBody] AllowanceCreateDto dto)
        {
            await _service.AddAllowanceAsync(dto);
            return Ok("Allowance added successfully for current month");
        }

        [HttpPost("deduction")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> AddDeduction([FromBody] DeductionCreateDto dto)
        {
            await _service.AddDeductionAsync(dto);
            return Ok("Deduction added successfully for current month");
        }

        [HttpGet("payslip/current/{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetCurrentPayslip(int userId)
        {
            var result = await _service.GetCurrentPayslipAsync(userId);

            if (result == null)
                return NotFound("Payslip not found");

            return Ok(result);
        }
    }
}