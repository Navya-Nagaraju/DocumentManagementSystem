using Document_Management_System.Data;
using Document_Management_System.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document_Management_System.Tests.Services
{
    public class AuditServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _service;

        public AuditServiceTests()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _service = new AuditService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task LogAsync_Should_Save_Audit_Record()
        {
            // Arrange

            // Act

            await _service.LogAsync(
                1,
                "UPLOAD_DOCUMENT",
                "Uploaded Resume");

            // Assert

            _context.AuditLogs.Count()
                .ShouldBe(1);
        }

        [Fact]
        public async Task LogAsync_Should_Save_Correct_UserId()
        {
            // Arrange

            // Act

            await _service.LogAsync(
                25,
                "DOWNLOAD_DOCUMENT",
                "Downloaded Resume");

            // Assert

            var log =
                await _context.AuditLogs.FirstAsync();

            log.UserId.ShouldBe(25);
        }

        [Fact]
        public async Task LogAsync_Should_Save_Correct_Action()
        {
            // Arrange

            // Act

            await _service.LogAsync(
                2,
                "APPROVE_DOCUMENT",
                "Approved Passport");

            // Assert

            var log =
                await _context.AuditLogs.FirstAsync();

            log.Action.ShouldBe("APPROVE_DOCUMENT");
        }

        [Fact]
        public async Task LogAsync_Should_Save_Correct_Description()
        {
            // Arrange

            // Act

            await _service.LogAsync(
                3,
                "REJECT_DOCUMENT",
                "Rejected License");

            // Assert

            var log =
                await _context.AuditLogs.FirstAsync();

            log.Description.ShouldBe("Rejected License");
        }

        [Fact]
        public async Task LogAsync_Should_Set_ActionDate()
        {
            // Arrange

            // Act

            await _service.LogAsync(
                1,
                "UPLOAD_DOCUMENT",
                "Uploaded Resume");

            // Assert

            var log =
                await _context.AuditLogs.FirstAsync();

            log.ActionDate.ShouldNotBe(default);
        }

        [Fact]
        public async Task GetLogsAsync_Should_Return_All_Logs()
        {
            // Arrange

            await _service.LogAsync(
                1,
                "UPLOAD_DOCUMENT",
                "Uploaded Resume");

            await _service.LogAsync(
                2,
                "DOWNLOAD_DOCUMENT",
                "Downloaded Resume");

            // Act

            var result =
                await _service.GetLogsAsync();

            // Assert

            result.Count.ShouldBe(2);
        }

        [Fact]
        public async Task GetLogsAsync_Should_Return_Correct_Action()
        {
            // Arrange

            await _service.LogAsync(
                1,
                "UPLOAD_DOCUMENT",
                "Uploaded Resume");

            // Act

            var result =
                await _service.GetLogsAsync();

            // Assert

            result.First().Action
                .ShouldBe("UPLOAD_DOCUMENT");
        }

        [Fact]
        public async Task GetLogsAsync_Should_Return_Empty_List_When_No_Logs()
        {
            // Act

            var result =
                await _service.GetLogsAsync();

            // Assert

            result.ShouldNotBeNull();

            result.ShouldBeEmpty();
        }
    }
}
