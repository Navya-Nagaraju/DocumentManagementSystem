using Document_Management_System.DTOs;

namespace Document_Management_System.Interfaces
{
    public interface IDashboardService
    {
        Task<CandidateDashboardDTO> GetCandidateDashboardAsync(int userId);

        Task<AdminDashboardDTO> GetAdminDashboardAsync();
    }
}