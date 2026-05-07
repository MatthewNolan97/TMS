using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TMS_API.Controllers {
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class UploadsController : ControllerBase {
        private readonly IWebHostEnvironment _env;

        public UploadsController(IWebHostEnvironment env) {
            _env = env;
        }

        public class ToyImageUploadDto {
            public IFormFile File { get; set; }
        }

        [HttpPost("toy-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadToyImage([FromForm] ToyImageUploadDto dto) {
            try {
                if (dto?.File == null || dto.File.Length == 0)
                    return BadRequest(new { error = "No file uploaded." });

                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".webp" };

                var ext = Path.GetExtension(dto.File.FileName);
                if (!allowedExtensions.Contains(ext))
                    return BadRequest(new { error = "Invalid file type. Allowed: jpg, jpeg, png, webp." });

                if (dto.File.Length > 10 * 1024 * 1024)
                    return BadRequest(new { error = "File too large (max 10 MB)." });

                var uploadsFolder = Path.Combine(_env.ContentRootPath, "images", "toys");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var stream = System.IO.File.Create(filePath)) {
                    await dto.File.CopyToAsync(stream);
                }

                return Ok(new { fileName = uniqueFileName });
            } catch (Exception ex) {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("toy-image/{fileName}")]
        public IActionResult GetToyImage(string fileName) {
            var ext = Path.GetExtension(fileName);
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp" };

            if (string.IsNullOrWhiteSpace(fileName) || !allowedExtensions.Contains(ext))
                return NotFound();

            var filePath = Path.Combine(_env.ContentRootPath, "images", "toys", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = ext.ToLowerInvariant() switch {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }
    }
}
