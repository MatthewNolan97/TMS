using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using TMSTeacher.Controllers;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class AuthorizationAttributeTests
    {
        [Fact]
        public void HomeController_HasAuthorizeRoles_AdminOrTeacher()
        {
            var attr = typeof(HomeController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("Admin,Teacher", attr!.Roles);
        }

        [Theory]
        [InlineData(typeof(ToyController))]
        [InlineData(typeof(StudentController))]
        [InlineData(typeof(ToyImagesController))]
        public void TeacherControllers_HaveRequireTeacherRolePolicy(Type controllerType)
        {
            var attr = controllerType
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("RequireTeacherRole", attr!.Policy);
        }
    }
}
