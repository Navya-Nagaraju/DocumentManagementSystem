using Document_Management_System.DTOs;

namespace Document_Management_System.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDTO>>
      GetNotificationsAsync(int userId);

        Task MarkAsReadAsync(int notificationId);
    }
}
