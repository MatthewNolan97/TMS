using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMS_API.Controllers;

namespace TMS_TEST.TMS_API.ControllerTests {
    public class UploadsControllerTests {
        private static (UploadsController Controller, string Root) CreateControllerWithTempRoot() {
            var root = Path.Combine(Path.GetTempPath(), "tms_uploads_tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(root);

            return (new UploadsController(mockEnv.Object), root);
        }

        private static IFormFile MakeFormFile(byte[] bytes, string fileName, string contentType) {
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "File", fileName) {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private static Dictionary<string, object?> ToPropertyDictionary(object obj) {
            return obj.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }

        [Fact]
        public async Task UploadToyImage_ReturnsBadRequest_WhenNoFile() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var dto = new UploadsController.ToyImageUploadDto { File = null };
                var result = await controller.UploadToyImage(dto);
                Assert.IsType<BadRequestObjectResult>(result);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public async Task UploadToyImage_ReturnsBadRequest_WhenInvalidExtension() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var file = MakeFormFile(new byte[] { 1, 2, 3 }, "bad.txt", "text/plain");
                var dto = new UploadsController.ToyImageUploadDto { File = file };

                var result = await controller.UploadToyImage(dto);

                var bad = Assert.IsType<BadRequestObjectResult>(result);
                Assert.NotNull(bad.Value);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public async Task UploadToyImage_ReturnsBadRequest_WhenTooLarge() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var bigBytes = new byte[(10 * 1024 * 1024) + 1];
                var file = MakeFormFile(bigBytes, "big.jpg", "image/jpeg");
                var dto = new UploadsController.ToyImageUploadDto { File = file };

                var result = await controller.UploadToyImage(dto);

                var bad = Assert.IsType<BadRequestObjectResult>(result);
                Assert.NotNull(bad.Value);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public void GetToyImage_ReturnsNotFound_WhenBadExtension() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var result = controller.GetToyImage("x.exe");
                Assert.IsType<NotFoundResult>(result);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public void GetToyImage_ReturnsNotFound_WhenMissingFile() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var result = controller.GetToyImage("missing.png");
                Assert.IsType<NotFoundResult>(result);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public void GetToyImage_ReturnsPhysicalFile_WithCorrectContentType() {
            var (controller, root) = CreateControllerWithTempRoot();
            try {
                var folder = Path.Combine(root, "images", "toys");
                Directory.CreateDirectory(folder);

                var fileName = "sample.webp";
                var filePath = Path.Combine(folder, fileName);
                System.IO.File.WriteAllBytes(filePath, new byte[] { 0x52, 0x49, 0x46, 0x46 });

                var result = controller.GetToyImage(fileName);

                var physical = Assert.IsType<PhysicalFileResult>(result);
                Assert.Equal(filePath, physical.FileName);
                Assert.Equal("image/webp", physical.ContentType);
            } finally {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
