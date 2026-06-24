using Document_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(
            IAuditService auditService)
        {
            _auditService = auditService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult>
            GetLogs()
        {
            var logs =
                await _auditService
                    .GetLogsAsync();

            return Ok(logs);
        }
    }
}