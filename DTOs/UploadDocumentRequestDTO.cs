namespace Document_Management_System.DTOs
{
    public class UploadDocumentRequestDTO
    {
        public string DocumentType { get; set; }

        public IFormFile File { get; set; }
    }
}
