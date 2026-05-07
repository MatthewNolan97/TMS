using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests.Students
{
    public class StudentModelTests
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
                UserType = "Student"
            };
        }

        [Fact]
        public async Task Can_Create_And_Save_Student()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(1, "John");
            var student = new Student
            {
                StudentId = 1,
                UserId = 1,
                Placement = "Daycare 1",
                Year = 2,
                User = user
            };

            context.Users.Add(user);
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var savedStudent = await context.Students.FirstOrDefaultAsync(s => s.StudentId == 1);
            Assert.NotNull(savedStudent);
            Assert.Equal(1, savedStudent.StudentId);
            Assert.Equal("Daycare 1", savedStudent.Placement);
            Assert.Equal(2, savedStudent.Year);
        }

        [Fact]
        public async Task Student_Should_Load_User_Navigation()
        {
            using var context = GetInMemoryDbContext();

            var user = CreateValidUser(10, "Alice");
            var student = new Student
            {
                StudentId = 2,
                UserId = 10,
                User = user
            };

            context.Users.Add(user);
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var loadedStudent = await context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == 2);

            Assert.NotNull(loadedStudent);
            Assert.NotNull(loadedStudent.User);
        }

        [Fact]
        public async Task Student_Should_Save_And_Load_ToyLoans_Navigation()
        {
            using var context = GetInMemoryDbContext();

            var student = new Student
            {
                StudentId = 3,
                UserId = 3,
                User = CreateValidUser(3, "Sam")
            };

            var loan1 = new ToyLoan
            {
                LoanId = 1,
                ToyId = 10,
                StudentId = 3,
                BorrowDate = DateOnly.Parse("2024-01-01"),
                DueDate = DateOnly.Parse("2024-01-10"),
                Student = student,
                Toy = new Toy { ToyId = 10, Name = "Car" }
            };

            var loan2 = new ToyLoan
            {
                LoanId = 2,
                ToyId = 11,
                StudentId = 3,
                BorrowDate = DateOnly.Parse("2024-01-02"),
                DueDate = DateOnly.Parse("2024-01-11"),
                Student = student,
                Toy = new Toy { ToyId = 11, Name = "Puzzle" }
            };

            student.ToyLoans.Add(loan1);
            student.ToyLoans.Add(loan2);

            context.Students.Add(student);
            await context.SaveChangesAsync();

            var loadedStudent = await context.Students
                .Include(s => s.ToyLoans)
                .FirstOrDefaultAsync(s => s.StudentId == 3);

            Assert.NotNull(loadedStudent);
            Assert.Equal(2, loadedStudent.ToyLoans.Count);
            Assert.Contains(loadedStudent.ToyLoans, l => l.ToyId == 10);
            Assert.Contains(loadedStudent.ToyLoans, l => l.ToyId == 11);
        }

        [Fact]
        public void Student_Collections_Should_Initialize_Empty()
        {
            var student = new Student();
            Assert.Empty(student.ToyLoans);
        }

        [Fact]
        public void Student_Should_Allow_Null_Placement_And_Year()
        {
            var student = new Student
            {
                Placement = null,
                Year = null
            };

            Assert.Null(student.Placement);
            Assert.Null(student.Year);
        }
    }
}
