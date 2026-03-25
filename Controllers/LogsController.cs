using hr_crm.Authorization;
using hr_crm.DTO;
using hr_crm.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hr_crm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LoggingService _loggingService;

        public LogsController(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        // Only users with the LOGS_VIEW permission (or CRM_FULL_ACCESS) can access
        [Authorize]
        [HasPermission("LOGS_VIEW")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _loggingService.GetAll();
            var dto = logs.Select(l => new LogEntryDto
            {
                LogEntryId = l.LogEntryId,
                UserId = l.UserId,
                UserName = l.UserName,
                Action = l.Action,
                Details = l.Details,
                Timestamp = l.Timestamp
            }).ToList();

            return Ok(dto);
        }

        [Authorize]
        [HasPermission("LOGS_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var l = await _loggingService.GetById(id);
            if (l == null) return NotFound();

            var dto = new LogEntryDto
            {
                LogEntryId = l.LogEntryId,
                UserId = l.UserId,
                UserName = l.UserName,
                Action = l.Action,
                Details = l.Details,
                Timestamp = l.Timestamp
            };

            return Ok(dto);
        }
    }
}
