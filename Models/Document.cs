using Document_Management_System.Enums;
namespace Document_Management_System.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedDate { get; set; }
            = DateTime.UtcNow;

        public Status Status { get; set; } = Status.Pending;

        public string? Remarks { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
