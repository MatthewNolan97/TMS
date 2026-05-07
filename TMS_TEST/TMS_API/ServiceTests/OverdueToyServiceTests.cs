using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.DAL;
using TMS_API.Services;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMS_API.ServiceTests
{
    public class OverdueToyServiceTests
    {
        private static K40TmsDdContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<K40TmsDdContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new K40TmsDdContext(options);
        }

        [Fact]
        public async Task CheckAndSendOverdueNotificationsAsync_CreatesNotifications_ForOverdueLoans_TeachersAndStudents()
        {
            var dbName = $"OverdueToyServiceTests_{System.Guid.NewGuid():N}";
            await using var context = CreateInMemoryDb(dbName);

            var studentUser = new User { UserId = 10, UserType = "Student", Oid = "student-oid" };
            var teacherUser = new User { UserId = 20, UserType = "Teacher", Oid = "teacher-oid" };

            var student = new Student { StudentId = 1, UserId = studentUser.UserId, User = studentUser };
            var teacher = new Teacher { TeacherId = 1, UserId = teacherUser.UserId, User = teacherUser };

            var toy = new Toy { ToyId = 1, Name = "Ball", IsActive = true, IsAvailable = false };

            var overdueLoan = new ToyLoan
            {
                LoanId = 1,
                StudentId = student.StudentId,
                Student = student,
                ToyId = toy.ToyId,
                Toy = toy,
                BorrowDate = DateOnly.FromDateTime(System.DateTime.Today.AddDays(-10)),
                DueDate = DateOnly.FromDateTime(System.DateTime.Today.AddDays(-1)),
                ReturnDate = null
            };

            context.Users.AddRange(studentUser, teacherUser);
            context.Students.Add(student);
            context.Teachers.Add(teacher);
            context.Toys.Add(toy);
            context.ToyLoans.Add(overdueLoan);
            await context.SaveChangesAsync();

            var createdNotifications = new System.Collections.Generic.List<Notification>();
            var mockRepo = new Mock<INotificationRepo>();
            mockRepo.Setup(r => r.Create(It.IsAny<Notification>()))
                .Returns<Notification>(n =>
                {
                    createdNotifications.Add(n);
                    return n;
                });

            var mockLogger = new Mock<ILogger<OverdueToyService>>();
            var service = new OverdueToyService(context, mockRepo.Object, mockLogger.Object);

            await service.CheckAndSendOverdueNotificationsAsync();

            Assert.Contains(createdNotifications, n => n.RecipientId == studentUser.UserId && n.Type == "Overdue");
            Assert.Contains(createdNotifications, n => n.RecipientId == teacherUser.UserId && n.Type == "OverdueAlert");
        }

        [Fact]
        public async Task SendEarlyWarningNotificationsAsync_CreatesNotifications_ForLoansDueWithinTwoDays()
        {
            var dbName = $"OverdueToyServiceTests_Warn_{System.Guid.NewGuid():N}";
            await using var context = CreateInMemoryDb(dbName);

            var studentUser = new User { UserId = 11, UserType = "Student", Oid = "student-oid" };
            var student = new Student { StudentId = 2, UserId = studentUser.UserId, User = studentUser };
            var toy = new Toy { ToyId = 2, Name = "Truck", IsActive = true, IsAvailable = false };

            var dueSoonLoan = new ToyLoan
            {
                LoanId = 2,
                StudentId = student.StudentId,
                Student = student,
                ToyId = toy.ToyId,
                Toy = toy,
                BorrowDate = DateOnly.FromDateTime(System.DateTime.Today.AddDays(-1)),
                DueDate = DateOnly.FromDateTime(System.DateTime.Today.AddDays(2)),
                ReturnDate = null
            };

            context.Users.Add(studentUser);
            context.Students.Add(student);
            context.Toys.Add(toy);
            context.ToyLoans.Add(dueSoonLoan);
            await context.SaveChangesAsync();

            var createdNotifications = new System.Collections.Generic.List<Notification>();
            var mockRepo = new Mock<INotificationRepo>();
            mockRepo.Setup(r => r.Create(It.IsAny<Notification>()))
                .Returns<Notification>(n =>
                {
                    createdNotifications.Add(n);
                    return n;
                });

            var mockLogger = new Mock<ILogger<OverdueToyService>>();
            var service = new OverdueToyService(context, mockRepo.Object, mockLogger.Object);

            await service.SendEarlyWarningNotificationsAsync();

            Assert.Contains(createdNotifications, n => n.RecipientId == studentUser.UserId && n.Type == "EarlyWarning");
        }
    }
}
