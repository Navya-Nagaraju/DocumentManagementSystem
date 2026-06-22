using Document_Management_System.Enums;
namespace Document_Management_System.DTOs
{
    public class ReviewDocumentDTO
    {
        public Status Status { get; set; }

        public string? Remarks { get; set; }
    }
}
