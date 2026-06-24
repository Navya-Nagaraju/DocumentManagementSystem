using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Document_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            int userId,
            string action,
            string description)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Description = description
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLogDTO>>
            GetLogsAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(x => x.ActionDate)
                .Select(x =>
                    new AuditLogDTO
                    {
                        Action = x.Action,
                        Description = x.Description,
                        ActionDate = x.ActionDate
                    })
                .ToListAsync();
        }
    }
}