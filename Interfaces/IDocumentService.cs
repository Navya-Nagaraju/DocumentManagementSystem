using Document_Management_System.DTOs;

namespace Document_Management_System.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDTO> UploadAsync(
      UploadDocumentRequestDTO request,
      int userId);

        Task<List<DocumentResponseDTO>> GetMyDocumentsAsync(
            int userId);

        Task<List<DocumentResponseDTO>> GetAllDocumentsAsync();

        Task<bool> ReviewDocumentAsync(
        int documentId,
        ReviewDocumentDTO dto);
    }
}
