using Document_Management_System.Data;
using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocumentManagementSystem.Tests.UnitTests
{
    public class AuditServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task LogAsync_ShouldCreateAuditLog()
        {
            // Arrange
            var context = GetDbContext();

            var service = new AuditService(context);

            // Act
            await service.LogAsync(
                1,
                "UPLOAD_DOCUMENT",
                "Uploaded Resume");

            // Assert
            context.AuditLogs.Count().Should().Be(1);
        }

        [Fact]
        public async Task LogAsync_ShouldStoreCorrectValues()
        {
            // Arrange
            var context = GetDbContext();

            var service = new AuditService(context);

            // Act
            await service.LogAsync(
                10,
                "DOWNLOAD_DOCUMENT",
                "Downloaded Resume");

            // Assert
            var log = context.AuditLogs.First();

            log.UserId.Should().Be(10);
            log.Action.Should().Be("DOWNLOAD_DOCUMENT");
            log.Description.Should().Be("Downloaded Resume");
        }

        [Fact]
        public async Task GetLogsAsync_ShouldReturnEmptyList_WhenNoLogsExist()
        {
            // Arrange
            var context = GetDbContext();

            var service = new AuditService(context);

            // Act
            var result = await service.GetLogsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLogsAsync_ShouldReturnAllLogs()
        {
            // Arrange
            var context = GetDbContext();

            context.AuditLogs.AddRange(

                new AuditLog
                {
                    UserId = 1,
                    Action = "UPLOAD_DOCUMENT",
                    Description = "Resume Uploaded"
                },

                new AuditLog
                {
                    UserId = 2,
                    Action = "DOWNLOAD_DOCUMENT",
                    Description = "Passport Downloaded"
                });

            await context.SaveChangesAsync();

            var service = new AuditService(context);

            // Act
            var result = await service.GetLogsAsync();

            // Assert
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetLogsAsync_ShouldReturnLogsOrderedByActionDateDescending()
        {
            // Arrange
            var context = GetDbContext();

            context.AuditLogs.Add(
                new AuditLog
                {
                    UserId = 1,
                    Action = "OLD",
                    Description = "Old Log",
                    ActionDate = DateTime.UtcNow.AddDays(-1)
                });

            context.AuditLogs.Add(
                new AuditLog
                {
                    UserId = 1,
                    Action = "NEW",
                    Description = "New Log",
                    ActionDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var service = new AuditService(context);

            // Act
            var result = await service.GetLogsAsync();

            // Assert
            result.First().Action.Should().Be("NEW");
            result.Last().Action.Should().Be("OLD");
        }
    }
}