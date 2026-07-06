using Document_Management_System.Data;
using Document_Management_System.Enums;
using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Xunit;

namespace DocumentManagementSystem.Tests.UnitTests
{
    public class DashboardServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCandidateDashboardAsync_ShouldReturnZeroCounts_WhenUserHasNoDocuments()
        {
            // Arrange
            var context = GetDbContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetCandidateDashboardAsync(1);

            // Assert
            result.TotalDocuments.Should().Be(0);
            result.ApprovedDocuments.Should().Be(0);
            result.RejectedDocuments.Should().Be(0);
            result.PendingDocuments.Should().Be(0);
            result.CompletionPercentage.Should().Be(0);
        }

        [Fact]
        public async Task GetCandidateDashboardAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var context = GetDbContext();

            context.Documents.AddRange(
                new Document
                {
                    UserId = 1,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Pending
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Rejected
                },
                new Document
                {
                    UserId = 2,
                    Status = Status.Approved
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetCandidateDashboardAsync(1);

            // Assert
            result.TotalDocuments.Should().Be(3);
            result.ApprovedDocuments.Should().Be(1);
            result.PendingDocuments.Should().Be(1);
            result.RejectedDocuments.Should().Be(1);
        }

        [Fact]
        public async Task GetCandidateDashboardAsync_ShouldCalculateCompletionPercentage()
        {
            // Arrange
            var context = GetDbContext();

            context.Documents.AddRange(
                new Document
                {
                    UserId = 1,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Pending
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Rejected
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetCandidateDashboardAsync(1);

            // Assert
            result.CompletionPercentage.Should().Be(50);
        }

        [Fact]
        public async Task GetAdminDashboardAsync_ShouldReturnZeroCounts_WhenDatabaseIsEmpty()
        {
            // Arrange
            var context = GetDbContext();
            var service = new DashboardService(context);

            // Act
            var result = await service.GetAdminDashboardAsync();

            // Assert
            result.TotalUsers.Should().Be(0);
            result.TotalDocuments.Should().Be(0);
            result.ApprovedDocuments.Should().Be(0);
            result.PendingDocuments.Should().Be(0);
            result.RejectedDocuments.Should().Be(0);
        }

        [Fact]
        public async Task GetAdminDashboardAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            var context = GetDbContext();

            context.Users.AddRange(
                new User
                {
                    FullName = "John",
                    Email = "john@test.com",
                    PasswordHash = "123",
                    Role = "Candidate"
                },
                new User
                {
                    FullName = "Admin",
                    Email = "admin@test.com",
                    PasswordHash = "123",
                    Role = "Admin"
                });

            context.Documents.AddRange(
                new Document
                {
                    UserId = 1,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 1,
                    Status = Status.Pending
                },
                new Document
                {
                    UserId = 2,
                    Status = Status.Rejected
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetAdminDashboardAsync();

            // Assert
            result.TotalUsers.Should().Be(2);
            result.TotalDocuments.Should().Be(3);
            result.ApprovedDocuments.Should().Be(1);
            result.PendingDocuments.Should().Be(1);
            result.RejectedDocuments.Should().Be(1);
        }

        [Fact]
        public async Task GetCandidateDashboardAsync_ShouldIgnoreOtherUsersDocuments()
        {
            // Arrange
            var context = GetDbContext();

            context.Documents.AddRange(
                new Document
                {
                    UserId = 1,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 2,
                    Status = Status.Approved
                },
                new Document
                {
                    UserId = 2,
                    Status = Status.Pending
                });

            await context.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetCandidateDashboardAsync(1);

            // Assert
            result.TotalDocuments.Should().Be(1);
            result.ApprovedDocuments.Should().Be(1);
            result.PendingDocuments.Should().Be(0);
            result.RejectedDocuments.Should().Be(0);
        }
    }
}