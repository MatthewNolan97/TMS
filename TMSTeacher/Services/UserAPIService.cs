using System.Net.Http.Json;
using System.Text.Json;
using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
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

      public async Task<User> CreateUserAsync(User user)
{
    if (user == null)
        throw new ArgumentNullException(nameof(user));

    var allowedTypes = new[] { "Student", "Teacher" };
    if (!allowedTypes.Contains(user.UserType))
    {
        throw new ArgumentException($"Invalid UserType. Allowed values are: {string.Join(", ", allowedTypes)}");
    }

    try
    {
        _logger.LogInformation($"Creating user with OID: {user.Oid} and type: {user.UserType}");
        var response = await _httpClient.PostAsJsonAsync("Users", user);
        response.EnsureSuccessStatusCode();
        
        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        _logger.LogInformation($"Successfully created user with ID: {createdUser?.UserId} and OID: {user.Oid}");
        return createdUser;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error creating user with OID: {user.Oid}. Error: {ex.Message}");
        throw;
    }
}
    }
}