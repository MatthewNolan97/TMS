using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace TMSTeacher.Services
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationDelegatingHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(
            ITokenAcquisition tokenAcquisition,
            IConfiguration configuration,
             ILogger<AuthenticationDelegatingHandler> logger,
            IHttpContextAccessor httpContextAccessor) 
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var apiClientId = _configuration["Api:ClientId"];
                var scope = $"api://{apiClientId}/User_Access";

                _logger.LogInformation($"Attempting to acquire token for scope: {scope}");

                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("No authenticated user found in HttpContext");
                    return await base.SendAsync(request, cancellationToken);
                }

                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
                    new[] { scope },
                    user: user); 

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                _logger.LogInformation("Token acquired successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to acquire token: {ex.Message}");
                throw;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}