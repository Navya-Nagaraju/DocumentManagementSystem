using Document_Management_System.Data;
using Document_Management_System.Models;
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
    
        public class NotificationServiceTests : IDisposable
        {
            private readonly ApplicationDbContext _context;
            private readonly NotificationService _service;

            public NotificationServiceTests()
            {
                var options =
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

                _context = new ApplicationDbContext(options);

                _service = new NotificationService(_context);
            }

            public void Dispose()
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }

            [Fact]
            public async Task GetNotificationsAsync_Should_Return_User_Notifications()
            {
                // Arrange

                _context.Notifications.AddRange(
                    new Notification
                    {
                        UserId = 1,
                        Message = "Approved",
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = 2,
                        Message = "Rejected",
                        IsRead = false
                    });

                await _context.SaveChangesAsync();

                // Act

                var result =
                    await _service.GetNotificationsAsync(1);

                // Assert

                result.Count.ShouldBe(1);

                result.First().Message
                    .ShouldBe("Approved");
            }

            [Fact]
            public async Task GetNotificationsAsync_Should_Return_Empty_List_When_User_Has_No_Notifications()
            {
                // Act

                var result =
                    await _service.GetNotificationsAsync(100);

                // Assert

                result.ShouldNotBeNull();

                result.ShouldBeEmpty();
            }

            [Fact]
            public async Task GetNotificationsAsync_Should_Return_Multiple_Notifications()
            {
                // Arrange

                _context.Notifications.AddRange(
                    new Notification
                    {
                        UserId = 1,
                        Message = "Approved"
                    },
                    new Notification
                    {
                        UserId = 1,
                        Message = "Downloaded"
                    },
                    new Notification
                    {
                        UserId = 2,
                        Message = "Rejected"
                    });

                await _context.SaveChangesAsync();

                // Act

                var result =
                    await _service.GetNotificationsAsync(1);

                // Assert

                result.Count.ShouldBe(2);
            }

            [Fact]
            public async Task GetNotificationsAsync_Should_Return_Correct_Message()
            {
                // Arrange

                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = 1,
                        Message = "Document Approved"
                    });

                await _context.SaveChangesAsync();

                // Act

                var result =
                    await _service.GetNotificationsAsync(1);

                // Assert

                result.First().Message
                    .ShouldBe("Document Approved");
            }

            [Fact]
            public async Task MarkAsReadAsync_Should_Update_IsRead()
            {
                // Arrange

                var notification =
                    new Notification
                    {
                        UserId = 1,
                        Message = "Approved",
                        IsRead = false
                    };

                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                // Act

                await _service.MarkAsReadAsync(notification.Id);

                // Assert

                var saved =
                    await _context.Notifications
                        .FindAsync(notification.Id);

                saved.ShouldNotBeNull();

                saved.IsRead.ShouldBeTrue();
            }

            [Fact]
            public async Task MarkAsReadAsync_Should_Not_Throw_When_Notification_Not_Found()
            {
                // Act

                var exception =
                    await Record.ExceptionAsync(() =>
                        _service.MarkAsReadAsync(999));

                // Assert

                exception.ShouldBeNull();
            }

            [Fact]
            public async Task MarkAsReadAsync_Should_Not_Modify_Other_Notifications()
            {
                // Arrange

                var first =
                    new Notification
                    {
                        UserId = 1,
                        Message = "Approved",
                        IsRead = false
                    };

                var second =
                    new Notification
                    {
                        UserId = 1,
                        Message = "Rejected",
                        IsRead = false
                    };

                _context.Notifications.AddRange(
                    first,
                    second);

                await _context.SaveChangesAsync();

                // Act

                await _service.MarkAsReadAsync(first.Id);

                // Assert

                var updated =
                    await _context.Notifications
                        .FindAsync(first.Id);

                var unchanged =
                    await _context.Notifications
                        .FindAsync(second.Id);

                updated.ShouldNotBeNull();
                unchanged.ShouldNotBeNull();

                updated.IsRead.ShouldBeTrue();

                unchanged.IsRead.ShouldBeFalse();
            }
        }
}
