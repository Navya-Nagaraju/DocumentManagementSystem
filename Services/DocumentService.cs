using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Document_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentService(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<DocumentResponseDTO> UploadAsync(
            UploadDocumentRequestDTO request,
            int userId)
        {
            var uploadsPath = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var storedFileName =
                $"{Guid.NewGuid()}_{request.File.FileName}";

            var filePath = Path.Combine(
                uploadsPath,
                storedFileName);

            using var stream =
                new FileStream(filePath, FileMode.Create);

            await request.File.CopyToAsync(stream);

            var document = new Document
            {
                DocumentType = request.DocumentType,
                OriginalFileName = request.File.FileName,
                StoredFileName = storedFileName,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                UploadedDate = DateTime.UtcNow,
                UserId = userId
            };

            _context.Documents.Add(document);

            await _context.SaveChangesAsync();

            return new DocumentResponseDTO
            {
                Id = document.Id,
                DocumentType = document.DocumentType,
                FileName = document.OriginalFileName,
                UploadedDate = document.UploadedDate,
                Status = document.Status,
                Remarks = document.Remarks
            };
        }

        public async Task<List<DocumentResponseDTO>>
            GetMyDocumentsAsync(int userId)
        {
            return await _context.Documents
                .Where(x => x.UserId == userId)
                .Select(x => new DocumentResponseDTO
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    FileName = x.OriginalFileName,
                    UploadedDate = x.UploadedDate,
                    Status = x.Status,
                    Remarks = x.Remarks
                })
                .ToListAsync();
        }

        public async Task<List<DocumentResponseDTO>>
            GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Select(x => new DocumentResponseDTO
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    FileName = x.OriginalFileName,
                    UploadedDate = x.UploadedDate,
                    Status = x.Status,
                    Remarks = x.Remarks
                })
                .ToListAsync();
        }

        public async Task<bool> ReviewDocumentAsync(
     int documentId,
     ReviewDocumentDTO dto)
        {
            var document =
                await _context.Documents.FindAsync(documentId);

            if (document == null)
                return false;

            document.Status = dto.Status;
            document.Remarks = dto.Remarks;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
