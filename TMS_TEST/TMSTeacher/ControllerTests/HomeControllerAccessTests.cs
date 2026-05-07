using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;  
using TMSTeacher.Controllers;
using TMSTeacher.Services;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class HomeControllerAccessTests
    {
        [Fact]
        public async Task Index_ReturnsForbid_WhenAuthenticatedButNotTeacherOrAdmin() 
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var notificationService = new Mock<INotificationService>();
            var userAPIService = new Mock<IUserAPIService>();


            userAPIService.Setup(x => x.GetUserByOidAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var controller = new HomeController(
                logger.Object,
                notificationService.Object,
                userAPIService.Object);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "student@example.test"),
                new Claim(ClaimTypes.Role, "Student"),
                new Claim("oid", "test-oid-123")  
            }, authenticationType: "TestAuth");

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            var result = await controller.Index(); 

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirectResult.ActionName);
        }

        [Fact]
        public void Login_ReturnsForbid_WhenAuthenticatedButNotTeacherOrAdmin()
        {
            var logger = new Mock<ILogger<HomeController>>();
            var notificationService = new Mock<INotificationService>();
            var userAPIService = new Mock<IUserAPIService>();

            var controller = new HomeController(
                logger.Object,
                notificationService.Object,
                userAPIService.Object);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "student@example.test"),
                new Claim(ClaimTypes.Role, "Student")
            }, authenticationType: "TestAuth");

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            var result = controller.Login();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirectResult.ActionName);
        }
    }
}