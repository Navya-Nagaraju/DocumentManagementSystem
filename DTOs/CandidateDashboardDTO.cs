namespace Document_Management_System.DTOs
{
    public class CandidateDashboardDTO
    {
        
            public int TotalDocuments { get; set; }

            public int ApprovedDocuments { get; set; }

            public int RejectedDocuments { get; set; }

            public int PendingDocuments { get; set; }

            public double CompletionPercentage { get; set; }
        
    }
}

