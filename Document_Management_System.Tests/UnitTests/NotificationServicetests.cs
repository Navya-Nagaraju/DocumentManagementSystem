using Document_Management_System.Data;
using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocumentManagementSystem.Tests.UnitTests
{
    public class NotificationServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnEmptyList_WhenNoNotificationsExist()
        {
            // Arrange
            var context = GetDbContext();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetNotificationsAsync(1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnOnlyUserNotifications()
        {
            // Arrange
            var context = GetDbContext();

            context.Notifications.AddRange(

                new Notification
                {
                    UserId = 1,
                    Message = "Resume Approved"
                },

                new Notification
                {
                    UserId = 1,
                    Message = "Passport Uploaded"
                },

                new Notification
                {
                    UserId = 2,
                    Message = "PAN Approved"
                });

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetNotificationsAsync(1);

            // Assert
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnNotificationsOrderedByCreatedDateDescending()
        {
            // Arrange
            var context = GetDbContext();

            context.Notifications.Add(
                new Notification
                {
                    UserId = 1,
                    Message = "Old Notification",
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                });

            context.Notifications.Add(
                new Notification
                {
                    UserId = 1,
                    Message = "New Notification",
                    CreatedDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            var result = await service.GetNotificationsAsync(1);

            // Assert
            result.First().Message.Should().Be("New Notification");
            result.Last().Message.Should().Be("Old Notification");
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldMarkNotificationAsRead()
        {
            // Arrange
            var context = GetDbContext();

            var notification = new Notification
            {
                UserId = 1,
                Message = "Resume Approved",
                IsRead = false
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            var service = new NotificationService(context);

            // Act
            await service.MarkAsReadAsync(notification.Id);

            // Assert
            context.Notifications
                .First()
                .IsRead
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldDoNothing_WhenNotificationDoesNotExist()
        {
            // Arrange
            var context = GetDbContext();

            var service = new NotificationService(context);

            // Act
            Func<Task> act = async () =>
                await service.MarkAsReadAsync(999);

            // Assert
            await act.Should().NotThrowAsync();

            context.Notifications.Count().Should().Be(0);
        }
    }
}