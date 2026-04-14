using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service;
using hr_crm.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;
        private readonly NotificationService _notification;

        public PayrollController(IPayrollService service, NotificationService notification)
        {
            _service = service;
            _notification = notification;
        }

        // =============================================
        // Generate Payroll (Draft)
        // =============================================
        [HttpPost("generate")]
        [HasPermission("PAYROLL_GENERATE")]
        public async Task<IActionResult> GeneratePayroll([FromBody] PayrollCreateDto dto)
        {
            var (result, error) = await _service.GeneratePayrollAsync(dto);

            if (result == null)
                return BadRequest(new { Message = error });

            return Ok(result);
        }

        // =============================================
        // Get All Payrolls
        // =============================================
        [HttpGet("all")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetPayroll()
        {
            var result = await _service.GetAllPayrollAsync();
            return Ok(result);
        }

        // =============================================
        // Get Payroll by User
        // =============================================
        [HttpGet("{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetPayrollByUserId(int userId)
        {
            var result = await _service.GetPayrollByUserIdAsync(userId);
            return Ok(result);
        }

        // =============================================
        // Update Payroll (Draft only)
        // =============================================
        [HttpPut("{payrollId}")]
        [HasPermission("PAYROLL_UPDATE")]
        public async Task<IActionResult> UpdatePayroll(int payrollId, [FromBody] PayrollCreateDto dto)
        {
            var result = await _service.UpdatePayrollAsync(payrollId, dto);

            if (result == null)
                return BadRequest(new { Message = "Payroll not found or cannot be edited. Only Draft payrolls can be updated." });

            return Ok(result);
        }

        // =============================================
        // Delete Payroll
        // =============================================
        [HttpDelete("{payrollId}")]
        [HasPermission("PAYROLL_DELETE")]
        public async Task<IActionResult> DeletePayroll(int payrollId)
        {
            var result = await _service.DeletePayrollAsync(payrollId);

            if (!result)
                return NotFound(new { Message = "Payroll record not found." });

            return Ok(new { Message = "Payroll deleted successfully." });
        }

        // =============================================
        // Add Allowance
        // =============================================
        [HttpPost("allowance")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> AddAllowance([FromBody] AllowanceCreateDto dto)
        {
            await _service.AddAllowanceAsync(dto);
            return Ok(new { Message = "Allowance added for current month." });
        }

        // =============================================
        // Add Deduction
        // =============================================
        [HttpPost("deduction")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> AddDeduction([FromBody] DeductionCreateDto dto)
        {
            await _service.AddDeductionAsync(dto);
            return Ok(new { Message = "Deduction added for current month." });
        }

        // =============================================
        // Get Current Payslip (itemized)
        // =============================================
        [HttpGet("payslip/current/{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetCurrentPayslip(int userId)
        {
            var result = await _service.GetCurrentPayslipAsync(userId);

            if (result == null)
                return NotFound(new { Message = "Payslip not found for current month." });

            return Ok(result);
        }

        // =============================================
        // Approve Payroll (Draft → Approved)
        // =============================================
        [HttpPut("{payrollId}/approve")]
        [HasPermission("PAYROLL_UPDATE")]
        public async Task<IActionResult> ApprovePayroll(int payrollId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var approvedBy = int.Parse(userIdClaim.Value);
            var (success, error) = await _service.ApprovePayrollAsync(payrollId, approvedBy);

            if (!success)
                return BadRequest(new { Message = error });

            // Notify the employee
            var payroll = await _service.GetPayrollByUserIdAsync(0); // get via id below
            var allPayrolls = await _service.GetAllPayrollAsync();
            var p = allPayrolls.FirstOrDefault(x => x.PayrollId == payrollId);
            if (p != null)
            {
                await _notification.CreateNotification(
                    p.UserId,
                    "Payroll Approved",
                    $"Your payroll for {p.Month:MMMM yyyy} has been approved. Net salary: ₹{p.NetSalary:N2}.",
                    "Payroll",
                    payrollId
                );
            }

            return Ok(new { Message = "Payroll approved successfully.", PayrollId = payrollId });
        }

        // =============================================
        // Mark as Paid (Approved → Paid)
        // =============================================
        [HttpPut("{payrollId}/mark-paid")]
        [HasPermission("PAYROLL_UPDATE")]
        public async Task<IActionResult> MarkAsPaid(int payrollId)
        {
            var (success, error) = await _service.MarkAsPaidAsync(payrollId);

            if (!success)
                return BadRequest(new { Message = error });

            // Notify the employee
            var allPayrolls = await _service.GetAllPayrollAsync();
            var p = allPayrolls.FirstOrDefault(x => x.PayrollId == payrollId);
            if (p != null)
            {
                await _notification.CreateNotification(
                    p.UserId,
                    "Salary Paid",
                    $"Your salary for {p.Month:MMMM yyyy} has been paid. Amount: ₹{p.NetSalary:N2}.",
                    "Payroll",
                    payrollId
                );
            }

            return Ok(new { Message = "Payroll marked as paid.", PayrollId = payrollId });
        }


        // =============================================
        // Set Salary Configuration (HR sets once per employee)
        // =============================================
        [HttpPost("salary-config")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> SetSalaryConfig([FromBody] SalaryConfigDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var setBy = int.Parse(userIdClaim.Value);
            var result = await _service.SetSalaryConfigAsync(dto, setBy);

            return Ok(new { Message = "Salary configuration saved. Payroll will auto-generate on the 1st of every month.", Config = result });
        }

        // =============================================
        // Get Salary Config for a user
        // =============================================
        [HttpGet("salary-config/{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetSalaryConfig(int userId)
        {
            var result = await _service.GetSalaryConfigAsync(userId);
            if (result == null)
                return NotFound(new { Message = "No salary configuration found for this employee." });

            return Ok(result);
        }

        // =============================================
        // Get All Salary Configs
        // =============================================
        [HttpGet("salary-config/all")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetAllSalaryConfigs()
        {
            var result = await _service.GetAllSalaryConfigsAsync();
            return Ok(result);
        }

        // =============================================
        // Manually trigger auto payroll (for testing)
        // =============================================
        [HttpPost("auto-generate")]
        [HasPermission("PAYROLL_GENERATE")]
        public async Task<IActionResult> TriggerAutoGenerate()
        {
            var (generated, skipped) = await _service.AutoGeneratePayrollForAllAsync();
            return Ok(new
            {
                Message = "Auto payroll generation completed.",
                Generated = generated,
                Skipped = skipped
            });
        }

        // =============================================
        // Create Bonus
        // =============================================
        [HttpPost("bonus")]
        [HasPermission("PAYROLL_CREATE")]
        public async Task<IActionResult> CreateBonus([FromBody] BonusCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var createdBy = int.Parse(userIdClaim.Value);
            var (result, error) = await _service.CreateBonusAsync(dto, createdBy);

            if (result == null)
                return BadRequest(new { Message = error });

            return Ok(new { Message = "Bonus created. Pending approval.", Bonus = result });
        }

        // =============================================
        // Approve Bonus (Pending → Approved)
        // =============================================
        [HttpPut("bonus/{id}/approve")]
        [HasPermission("PAYROLL_UPDATE")]
        public async Task<IActionResult> ApproveBonus(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var approvedBy = int.Parse(userIdClaim.Value);
            var (success, error) = await _service.ApproveBonusAsync(id, approvedBy);

            if (!success)
                return BadRequest(new { Message = error });

            // Notify employee
            var bonuses = await _service.GetAllBonusesAsync();
            var b = bonuses.FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                await _notification.CreateNotification(
                    b.UserId,
                    "Bonus Approved",
                    $"Your {b.BonusType} bonus of ₹{b.Amount:N2} for {new DateTime(b.Year, b.Month, 1):MMMM yyyy} has been approved.",
                    "Payroll",
                    id
                );
            }

            return Ok(new { Message = "Bonus approved. It will be included in the next payroll.", BonusId = id });
        }

        // =============================================
        // Get All Bonuses (HR)
        // =============================================
        [HttpGet("bonus/all")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetAllBonuses()
        {
            var result = await _service.GetAllBonusesAsync();
            return Ok(result);
        }

        // =============================================
        // Get Bonuses by User
        // =============================================
        [HttpGet("bonus/{userId}")]
        [HasPermission("PAYROLL_VIEW")]
        public async Task<IActionResult> GetBonusesByUser(int userId)
        {
            var result = await _service.GetBonusesByUserAsync(userId);
            return Ok(result);
        }

    }
}
