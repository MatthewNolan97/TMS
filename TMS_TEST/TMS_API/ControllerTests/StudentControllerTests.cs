using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS_API.Controllers;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Models;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class StudentControllerTests
    {
        [Fact]
        public void Get_ReturnsMappedStudents()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();

            var students = new List<Student>
            {
                new Student { StudentId = 1, Placement = "A", Year = 1 },
                new Student { StudentId = 2, Placement = "B", Year = 2 }
            };

            var studentDtos = new List<StudentDTO>
            {
                new StudentDTO { StudentId = 1, Placement = "A", Year = 1 },
                new StudentDTO { StudentId = 2, Placement = "B", Year = 2 }
            };

            mockRepo.Setup(r => r.GetAll()).Returns(students);
            mockMapper.Setup(m => m.Map<IEnumerable<StudentDTO>>(students))
                      .Returns(studentDtos);

            var controller = new StudentsController(mockRepo.Object, mockMapper.Object, mockToyRepo.Object);

            var result = controller.Get().Result;

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStudents = Assert.IsAssignableFrom<IEnumerable<StudentDTO>>(okResult.Value);

            Assert.Collection(returnedStudents,
                s => Assert.Equal(1, s.StudentId),
                s => Assert.Equal(2, s.StudentId));
        }

        [Fact]
        public void Get_ById_ReturnsNotFound_WhenNotExists()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();

            mockRepo.Setup(r => r.FindById(999)).Returns((Student)null);

            var controller = new StudentsController(mockRepo.Object, mockMapper.Object, mockToyRepo.Object);

            var result = controller.Get(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_ById_ReturnsMappedStudent()
        {
            var mockRepo = new Mock<IStudentRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockToyRepo = new Mock<IToyRepo>();

            var student = new Student
            {
                StudentId = 5,
                Placement = "C",
                Year = 3
            };

            var dto = new StudentDTO
            {
                StudentId = 5,
                Placement = "C",
                Year = 3
            };

            mockRepo.Setup(r => r.FindById(5)).Returns(student);
            mockMapper.Setup(m => m.Map<StudentDTO>(student)).Returns(dto);

            var controller = new StudentsController(mockRepo.Object, mockMapper.Object, mockToyRepo.Object);

            var result = controller.Get(5).Result;

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStudent = Assert.IsType<StudentDTO>(okResult.Value);

            Assert.Equal(5, returnedStudent.StudentId);
            Assert.Equal("C", returnedStudent.Placement);
            Assert.Equal(3, returnedStudent.Year);
        }
    }
}
