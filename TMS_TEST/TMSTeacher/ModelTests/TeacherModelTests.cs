using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests.Teachers
{
    public class TeacherModelTests
    {
        private K40TmsDdContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<K40TmsDdContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new K40TmsDdContext(options);
        }

        private User CreateValidUser(int id, string firstName)
        {
            return new User
            {
                UserId = id,
                UserType = "Teacher"
            };
        }

        [Fact]
        public async Task Can_Create_And_Save_Teacher()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(1, "John");
            var teacher = new Teacher { TeacherId = 1, UserId = 1, User = user };

            context.Users.Add(user);
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var savedTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == 1);
            Assert.NotNull(savedTeacher);
            Assert.Equal(1, savedTeacher.TeacherId);
            Assert.Equal(1, savedTeacher.UserId);
        }

        [Fact]
        public async Task Teacher_Should_Load_User_Navigation()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(10, "Alice");
            var teacher = new Teacher { TeacherId = 2, UserId = 10, User = user };

            context.Users.Add(user);
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var loadedTeacher = await context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == 2);

            Assert.NotNull(loadedTeacher);
            Assert.NotNull(loadedTeacher.User);
        }

        [Fact]
        public async Task Teacher_Should_Save_And_Load_Toys_Navigation()
        {
            using var context = GetInMemoryDbContext();

            var teacher = new Teacher
            {
                TeacherId = 3,
                UserId = 3,
                User = CreateValidUser(3, "Sam")
            };

            var toy1 = new Toy { ToyId = 100, Name = "Car" };
            var toy2 = new Toy { ToyId = 101, Name = "Puzzle" };

            teacher.Toys.Add(toy1);
            teacher.Toys.Add(toy2);

            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var loadedTeacher = await context.Teachers
                .Include(t => t.Toys)
                .FirstOrDefaultAsync(t => t.TeacherId == 3);

            Assert.NotNull(loadedTeacher);
            Assert.Equal(2, loadedTeacher.Toys.Count);
            Assert.Contains(loadedTeacher.Toys, t => t.Name == "Car");
            Assert.Contains(loadedTeacher.Toys, t => t.Name == "Puzzle");
        }

    }
}
