using Document_Management_System.Enums;

namespace Document_Management_System.DTOs
{
    public class SearchDocumentDTO
    {
        public string? CandidateName { get; set; }

        public string? DocumentType { get; set; }

        public Status? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
