using Document_Management_System.DTOs;

namespace Document_Management_System.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            int userId,
            string action,
            string description);

        Task<List<AuditLogDTO>>
            GetLogsAsync();
    }
}
