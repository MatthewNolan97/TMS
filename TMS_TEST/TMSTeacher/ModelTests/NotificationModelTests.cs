using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests.Notifications
{
    public class NotificationModelTests
    {
        private K40TmsDdContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<K40TmsDdContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new K40TmsDdContext(options);
        }

        private User CreateValidUser(int id, string name)
        {
            return new User
            {
                UserId = id,
                UserType = "User"
            };
        }

        [Fact]
        public async Task Can_Create_And_Save_Notification()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(1, "John");
            var notification = new Notification
            {
                NotificationId = 1,
                RecipientId = 1,
                Message = "Test message",
                DateSent = DateTime.UtcNow,
                Type = "Info",
                Recipient = user
            };

            context.Users.Add(user);
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var savedNotification = await context.Notifications
                .Include(n => n.Recipient)
                .FirstOrDefaultAsync(n => n.NotificationId == 1);

            Assert.NotNull(savedNotification);
            Assert.Equal("Test message", savedNotification.Message);
            Assert.Equal("Info", savedNotification.Type);
            Assert.NotNull(savedNotification.Recipient);
        }

        [Fact]
        public async Task Notification_Should_Allow_Null_DateSent_And_Type()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(2, "Alice");
            var notification = new Notification
            {
                NotificationId = 2,
                RecipientId = 2,
                Message = "Nullable test",
                Recipient = user
            };

            context.Users.Add(user);
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var savedNotification = await context.Notifications
                .Include(n => n.Recipient)
                .FirstOrDefaultAsync(n => n.NotificationId == 2);

            Assert.NotNull(savedNotification);
            Assert.Null(savedNotification.DateSent);
            Assert.Null(savedNotification.Type);
            Assert.NotNull(savedNotification.Recipient);
        }

    }
}
