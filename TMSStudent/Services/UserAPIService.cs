using System.Net.Http.Json;
using System.Text.Json;
using TMS_SharedLibrary.Models;

namespace TMSStudent.Services
{
    public class UserAPIService : IUserAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserAPIService> _logger;

        public UserAPIService(HttpClient httpClient, ILogger<UserAPIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<User> GetUserByOidAsync(string oid)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>($"Users/oid/{oid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user with OID: {oid}");
                return null;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Users", user);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating user with OID: {user.Oid}");
                throw;
            }
        }
    }
}
