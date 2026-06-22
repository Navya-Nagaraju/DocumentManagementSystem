using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Microsoft.EntityFrameworkCore;
using Document_Management_System.Enums;

namespace Document_Management_System.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateDashboardDTO> GetCandidateDashboardAsync(int userId)
        {
            var documents = await _context.Documents
                .Where(x => x.UserId == userId)
                .ToListAsync();

            int total = documents.Count;

            int approved = documents.Count(x =>
                x.Status == Status.Approved);

            int rejected = documents.Count(x =>
                x.Status == Status.Rejected);

            int pending = documents.Count(x =>
                x.Status == Status.Pending);

            double percentage = total == 0
                ? 0
                : ((double)approved / total) * 100;

            return new CandidateDashboardDTO
            {
                TotalDocuments = total,
                ApprovedDocuments = approved,
                RejectedDocuments = rejected,
                PendingDocuments = pending,
                CompletionPercentage = Math.Round(percentage, 2)
            };
        }

        public async Task<AdminDashboardDTO> GetAdminDashboardAsync()
        {
            return new AdminDashboardDTO
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalDocuments = await _context.Documents.CountAsync(),

                ApprovedDocuments = await _context.Documents
                    .CountAsync(x => x.Status == Status.Approved),

                RejectedDocuments = await _context.Documents
                    .CountAsync(x => x.Status == Status.Rejected),

                PendingDocuments = await _context.Documents
                    .CountAsync(x => x.Status == Status.Pending),
        };
        }

    }
}