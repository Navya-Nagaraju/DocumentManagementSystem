using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDTO>>
            GetNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new NotificationDTO
                {
                    Id = x.Id,
                    Message = x.Message,
                    IsRead = x.IsRead,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(
            int notificationId)
        {
            var notification =
                await _context.Notifications
                    .FindAsync(notificationId);

            if (notification == null)
                return;

            notification.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}
