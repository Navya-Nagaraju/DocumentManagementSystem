using Document_Management_System.Data;
using Document_Management_System.Enums;
using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document_Management_System.Tests.Services
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
                result.CompletionPercentage.Should().Be(33.33);
            }

            [Fact]
            public async Task GetCandidateDashboardAsync_ShouldReturnZero_WhenNoDocumentsExist()
            {
                // Arrange
                var context = GetDbContext();

                var service = new DashboardService(context);

                // Act
                var result = await service.GetCandidateDashboardAsync(1);

                // Assert
                result.TotalDocuments.Should().Be(0);
                result.ApprovedDocuments.Should().Be(0);
                result.PendingDocuments.Should().Be(0);
                result.RejectedDocuments.Should().Be(0);
                result.CompletionPercentage.Should().Be(0);
            }

            [Fact]
            public async Task GetCandidateDashboardAsync_ShouldOnlyReturnCurrentUsersDocuments()
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

            [Fact]
            public async Task GetAdminDashboardAsync_ShouldReturnCorrectStatistics()
            {
                // Arrange
                var context = GetDbContext();

                context.Users.AddRange(
                    new User
                    {
                        FullName = "John",
                        Email = "john@test.com",
                        PasswordHash = "123"
                    },
                    new User
                    {
                        FullName = "Jane",
                        Email = "jane@test.com",
                        PasswordHash = "123"
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
        }
}
