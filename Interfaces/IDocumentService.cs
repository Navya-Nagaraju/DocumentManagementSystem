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
        ReviewDocumentDTO dto,
        int adminId);

        Task<(byte[] FileBytes,
        string ContentType,
        string FileName)>
        DownloadAsync(int documentId, int userId, string role);

        Task<List<DocumentResponseDTO>> SearchAsync(
        SearchDocumentDTO request,
        int userId,
        string role);
    }
}
