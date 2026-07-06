using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Enums;
using Document_Management_System.Interfaces;
using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DocumentManagementSystem.Tests.UnitTests
{
    public class DocumentServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<IAuditService> _auditServiceMock;

        public DocumentServiceTests()
        {
            _environmentMock = new Mock<IWebHostEnvironment>();
            _auditServiceMock = new Mock<IAuditService>();

            _environmentMock
                .Setup(x => x.ContentRootPath)
                .Returns(Path.GetTempPath());
        }

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private IFormFile CreateTestFile(
            string fileName = "Resume.pdf",
            string content = "Dummy File")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            var stream = new MemoryStream(bytes);

            return new FormFile(stream,
                0,
                bytes.Length,
                "Data",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        [Fact]
        public async Task UploadAsync_ShouldUploadDocumentSuccessfully()
        {
            // Arrange

            var context = GetDbContext();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var request = new UploadDocumentRequestDTO
            {
                DocumentType = "Resume",
                File = CreateTestFile()
            };

            // Act

            var result = await service.UploadAsync(request, 1);

            // Assert

            result.Should().NotBeNull();

            result.DocumentType.Should().Be("Resume");

            context.Documents.Count().Should().Be(1);
        }

        [Fact]
        public async Task UploadAsync_ShouldSaveCorrectFileName()
        {
            var context = GetDbContext();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var request = new UploadDocumentRequestDTO
            {
                DocumentType = "Passport",
                File = CreateTestFile("Passport.pdf")
            };

            await service.UploadAsync(request, 5);

            var document = context.Documents.First();

            document.OriginalFileName
                .Should()
                .Be("Passport.pdf");
        }

        [Fact]
        public async Task UploadAsync_ShouldStoreCorrectUserId()
        {
            var context = GetDbContext();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var request = new UploadDocumentRequestDTO
            {
                DocumentType = "PAN",
                File = CreateTestFile()
            };

            await service.UploadAsync(request, 20);

            var document = context.Documents.First();

            document.UserId.Should().Be(20);
        }

        [Fact]
        public async Task UploadAsync_ShouldCreateAuditLog()
        {
            var context = GetDbContext();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var request = new UploadDocumentRequestDTO
            {
                DocumentType = "Resume",
                File = CreateTestFile()
            };

            await service.UploadAsync(request, 1);

            _auditServiceMock.Verify(x =>
                x.LogAsync(
                    1,
                    "UPLOAD_DOCUMENT",
                    It.IsAny<string>()),
                Moq.Times.Once);
        }

        [Fact]
        public async Task UploadAsync_ShouldGenerateStoredFileName()
        {
            var context = GetDbContext();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var request = new UploadDocumentRequestDTO
            {
                DocumentType = "Resume",
                File = CreateTestFile("Resume.pdf")
            };

            await service.UploadAsync(request, 1);

            var document = context.Documents.First();

            document.StoredFileName
                .Should()
                .Contain("Resume.pdf");
        }

        [Fact]
        public async Task GetMyDocumentsAsync_ShouldReturnOnlyLoggedInUserDocuments()
        {
            var context = GetDbContext();

            context.Documents.AddRange(

                new Document
                {
                    UserId = 1,
                    DocumentType = "Resume"
                },

                new Document
                {
                    UserId = 1,
                    DocumentType = "Passport"
                },

                new Document
                {
                    UserId = 2,
                    DocumentType = "PAN"
                });

            await context.SaveChangesAsync();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var result =
                await service.GetMyDocumentsAsync(1);

            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetMyDocumentsAsync_ShouldReturnEmptyList_WhenUserHasNoDocuments()
        {
            var context = GetDbContext();

            context.Documents.Add(
                new Document
                {
                    UserId = 2,
                    DocumentType = "Resume"
                });

            await context.SaveChangesAsync();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var result =
                await service.GetMyDocumentsAsync(1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMyDocumentsAsync_ShouldReturnCorrectDocumentType()
        {
            var context = GetDbContext();

            context.Documents.Add(
                new Document
                {
                    UserId = 1,
                    DocumentType = "Aadhar"
                });

            await context.SaveChangesAsync();

            var service = new DocumentService(
                context,
                _environmentMock.Object,
                _auditServiceMock.Object);

            var result =
                await service.GetMyDocumentsAsync(1);

            result.First().DocumentType
                .Should()
                .Be("Aadhar");
        }
    }
}