using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Document_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Document_Management_System.Enums;

namespace Document_Management_System.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IAuditService _auditService;

        public DocumentService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IAuditService auditService)
        {
            _context = context;
            _environment = environment;
            _auditService = auditService;
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
            await _auditService.LogAsync(
                userId,
                "UPLOAD_DOCUMENT",
                $"Uploaded {request.DocumentType}");

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
            ReviewDocumentDTO dto,
            int adminId)
            {
            var document =
                await _context.Documents.FindAsync(documentId);

            if (document == null)
                return false;

            document.Status = dto.Status;
            document.Remarks = dto.Remarks;

            string message = "";

            if (dto.Status == Status.Approved)
            {
                message =
                    $"Your document '{document.DocumentType}' has been approved.";
            }
            else if (dto.Status == Status.Rejected)
            {
                message =
                    $"Your document '{document.DocumentType}' has been rejected. Remarks: {dto.Remarks}";
            }

            _context.Notifications.Add(
                new Notification
                {
                    UserId = document.UserId,
                    Message = message
                });

            await _context.SaveChangesAsync();

            if (dto.Status == Status.Approved)
            {
                await _auditService.LogAsync(
                    adminId,
                    "APPROVE_DOCUMENT",
                    $"Approved document {document.Id}");
            }
            else if (dto.Status == Status.Rejected)
            {
                await _auditService.LogAsync(
                    adminId,
                    "REJECT_DOCUMENT",
                    $"Rejected document {document.Id}");
            }

            return true;
        }

        public async Task<(byte[] FileBytes,
                   string ContentType,
                   string FileName)>
                   DownloadAsync(
                   int documentId,
                   int userId,
                   string role)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(x => x.Id == documentId);

            if (document == null)
                throw new Exception("Document not found");

            // Candidate can only download own document
            if (role == "Candidate" &&
                document.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Access denied");
            }

            var uploadsPath = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            var filePath = Path.Combine(
                uploadsPath,
                document.StoredFileName);

            if (!File.Exists(filePath))
                throw new Exception("File not found");

            _context.AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = "DOWNLOAD_DOCUMENT",
                Description = $"User downloaded document with Id {documentId}",
                ActionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var bytes =
                await File.ReadAllBytesAsync(filePath);

            return (
                bytes,
                document.ContentType,
                document.OriginalFileName
            );
        }

        public async Task<List<DocumentResponseDTO>> SearchAsync(
    SearchDocumentDTO request,
    int userId,
    string role)
        {
            var query = _context.Documents
                .Include(x => x.User)
                .AsQueryable();

            // Candidate can see only own docs
            if (role == "Candidate")
            {
                query = query.Where(x => x.UserId == userId);
            }

            if (!string.IsNullOrEmpty(request.CandidateName))
            {
                query = query.Where(x =>
                    x.User!.FullName.Contains(request.CandidateName));
            }

            if (!string.IsNullOrEmpty(request.DocumentType))
            {
                query = query.Where(x =>
                    x.DocumentType.Contains(request.DocumentType));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x =>
                    x.Status == request.Status.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x =>
                    x.UploadedDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var endDate =
                    request.ToDate.Value.Date.AddDays(1);
                query = query.Where(x =>
                    x.UploadedDate <= request.ToDate.Value);
            }

            return await query
                .Select(x => new DocumentResponseDTO
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    Status = x.Status,
                    UploadedDate = x.UploadedDate,
                    Remarks = x.Remarks,
                    FileName = x.OriginalFileName
                })
                .ToListAsync();
        }
    }
}
