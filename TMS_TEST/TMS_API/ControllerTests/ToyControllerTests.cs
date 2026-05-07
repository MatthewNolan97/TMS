using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using TMS_API.Controllers;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMS_API.ControllerTests
{
    public class ToysControllerTests
    {
        [Fact]
        public void Get_ReturnsMappedToys()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var toys = new List<Toy>
            {
                new Toy { ToyId = 1, Name = "Ball", IsActive = true },
                new Toy { ToyId = 2, Name = "Truck", IsActive = true }
            };

            var toyDtos = new List<ToyDTO>
            {
                new ToyDTO { ToyId = 1, Name = "Ball" },
                new ToyDTO { ToyId = 2, Name = "Truck" }
            };

            mockRepo.Setup(r => r.GetAll()).Returns(toys);
            mockMapper.Setup(m => m.Map<IEnumerable<ToyDTO>>(toys)).Returns(toyDtos);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Get().Result;

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<ToyDTO>>(okResult.Value);

            Assert.Collection(returned,
                t => Assert.Equal(1, t.ToyId),
                t => Assert.Equal(2, t.ToyId));
        }

        [Fact]
        public void Get_ById_ReturnsNotFound_WhenToyDoesNotExist()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            mockRepo.Setup(r => r.FindById(999)).Returns((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Get(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_ById_ReturnsMappedToy()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var toy = new Toy { ToyId = 5, Name = "Doll", IsActive = true };
            var dto = new ToyDTO { ToyId = 5, Name = "Doll" };

            mockRepo.Setup(r => r.FindById(5)).Returns(toy);
            mockMapper.Setup(m => m.Map<ToyDTO>(toy)).Returns(dto);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Get(5).Result;

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ToyDTO>(okResult.Value);

            Assert.Equal(5, returned.ToyId);
            Assert.Equal("Doll", returned.Name);
        }

        [Fact]
        public void Create_AddsToy_AndReturnsCreated()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var dto = new ToyDTO { ToyId = 10, Name = "Puzzle" };
            var toy = new Toy { ToyId = 10, Name = "Puzzle", IsActive = false };

            mockMapper.Setup(m => m.Map<Toy>(dto)).Returns(toy);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Create(dto);

            mockRepo.Verify(r => r.Add(toy), Times.Once);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("Create", created.ActionName);
            Assert.Equal(toy.ToyId, created.RouteValues["id"]);
        }

        [Fact]
        public void Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var dto = new ToyDTO { ToyId = 2, Name = "Test" };

            var result = controller.Update(5, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void Update_ReturnsNotFound_WhenToyMissing()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            mockRepo.Setup(r => r.FindById(10)).Returns((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var dto = new ToyDTO { ToyId = 10, Name = "Blocks" };

            var result = controller.Update(10, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Update_UpdatesToy_AndReturnsNoContent()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var existing = new Toy { ToyId = 3, Name = "Car", IsActive = true };
            var updated = new Toy { ToyId = 3, Name = "Updated Car", IsActive = true };

            var dto = new ToyDTO { ToyId = 3, Name = "Updated Car" };

            mockRepo.Setup(r => r.FindById(3)).Returns(existing);
            mockMapper.Setup(m => m.Map<Toy>(dto)).Returns(updated);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Update(3, dto);

            mockRepo.Verify(r => r.Update(3, updated), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenMissing()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            mockRepo.Setup(r => r.FindById(77)).Returns((Toy)null);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Delete(77);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_DeletesToy_AndReturnsNoContent()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var toy = new Toy { ToyId = 44, Name = "Plane", IsAvailable = true, IsActive = true };

            mockRepo.Setup(r => r.FindById(44)).Returns(toy);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Delete(44);

            mockRepo.Verify(r => r.Delete(44), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_ReturnsBadRequest_WhenToyIsBorrowed()
        {
            var mockRepo = new Mock<IToyRepo>();
            var mockMapper = new Mock<IMapper>();

            var toy = new Toy {
                ToyId = 50,
                Name = "Train",
                IsAvailable = false,
                IsActive = true
            };

            mockRepo.Setup(r => r.FindById(50)).Returns(toy);

            var controller = new ToysController(mockRepo.Object, mockMapper.Object);

            var result = controller.Delete(50);

            Assert.IsType<BadRequestObjectResult>(result);
            mockRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }

    }
}