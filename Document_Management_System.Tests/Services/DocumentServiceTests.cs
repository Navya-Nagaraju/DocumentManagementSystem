using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Document_Management_System.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document_Management_System.Tests.Services
{
    public class DocumentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<IAuditService> _auditMock;
        private readonly DocumentService _service;

        private readonly string _contentRoot;

        public DocumentServiceTests()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context =
                new ApplicationDbContext(options);

            _environmentMock =
                new Mock<IWebHostEnvironment>();

            _auditMock =
                new Mock<IAuditService>();

            _contentRoot =
                Path.Combine(Path.GetTempPath(),
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(_contentRoot);

            _environmentMock
                .Setup(x => x.ContentRootPath)
                .Returns(_contentRoot);

            _service =
                new DocumentService(
                    _context,
                    _environmentMock.Object,
                    _auditMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();

            if (Directory.Exists(_contentRoot))
            {
                Directory.Delete(
                    _contentRoot,
                    true);
            }
        }

        private IFormFile CreateTestFile(
            string fileName = "Resume.pdf",
            string content = "Sample File")
        {
            var bytes =
                System.Text.Encoding.UTF8.GetBytes(content);

            var stream =
                new MemoryStream(bytes);

            return new FormFile(
                stream,
                0,
                bytes.Length,
                "file",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        [Fact]
        public async Task UploadAsync_Should_Save_Document()
        {
            // Arrange

            var request =
                new UploadDocumentRequestDTO
                {
                    DocumentType = "Resume",
                    File = CreateTestFile()
                };

            // Act

            var result =
                await _service.UploadAsync(
                    request,
                    1);

            // Assert

            result.ShouldNotBeNull();

            result.DocumentType.ShouldBe("Resume");

            _context.Documents.Count()
                .ShouldBe(1);

            var saved =
                await _context.Documents.FirstAsync();

            saved.DocumentType
                .ShouldBe("Resume");

            saved.UserId
                .ShouldBe(1);

            saved.OriginalFileName
                .ShouldBe("Resume.pdf");
        }

        [Fact]
        public async Task UploadAsync_Should_Create_Physical_File()
        {
            // Arrange

            var request =
                new UploadDocumentRequestDTO
                {
                    DocumentType = "Passport",
                    File = CreateTestFile(
                        "Passport.pdf")
                };

            // Act

            await _service.UploadAsync(
                request,
                1);

            // Assert

            var saved =
                await _context.Documents.FirstAsync();

            var uploads =
                Path.Combine(
                    _contentRoot,
                    "Uploads");

            var file =
                Path.Combine(
                    uploads,
                    saved.StoredFileName);

            File.Exists(file)
                .ShouldBeTrue();
        }

        [Fact]
        public async Task UploadAsync_Should_Call_Audit_Log()
        {
            // Arrange

            var request =
                new UploadDocumentRequestDTO
                {
                    DocumentType = "License",
                    File = CreateTestFile(
                        "License.pdf")
                };

            // Act

            await _service.UploadAsync(
                request,
                10);

            // Assert

            _auditMock.Verify(
                x => x.LogAsync(
                    10,
                    "UPLOAD_DOCUMENT",
                    "Uploaded License"),
                Times.Once);
        }
    }
}
