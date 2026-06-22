using Document_Management_System.Enums;

namespace Document_Management_System.DTOs
{
    public class DocumentResponseDTO
    {
        public int Id { get; set; }

        public string DocumentType { get; set; }

        public string FileName { get; set; }

        public DateTime UploadedDate { get; set; }

        public Status Status { get; set; }

        public string Remarks { get; set; }
    }
}
