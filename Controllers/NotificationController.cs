using Document_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService
            _notificationService;

        public NotificationController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult>
            GetNotifications()
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!
                    .Value);

            var result =
                await _notificationService
                    .GetNotificationsAsync(userId);

            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult>
            MarkAsRead(int id)
        {
            await _notificationService
                .MarkAsReadAsync(id);

            return Ok("Notification marked as read");
        }
    }
}
