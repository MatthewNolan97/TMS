using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace TMSTeacher.Controllers
{
    [Authorize(Policy = "RequireTeacherRole")]
    [Route("toy-images")]
    public class ToyImagesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ToyImagesController> _logger;

        public ToyImagesController(IHttpClientFactory httpClientFactory, ILogger<ToyImagesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TmsApi");
            _logger = logger;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> Get(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            try
            {
                var resp = await _httpClient.GetAsync($"Uploads/toy-image/{Uri.EscapeDataString(fileName)}");

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Image not found: {fileName}");
                    return NotFound();
                }

                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                var bytes = await resp.Content.ReadAsByteArrayAsync();

                return File(bytes, contentType);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                // Re-throw to trigger consent
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching image: {fileName}");
                return NotFound();
            }
        }
    }
}