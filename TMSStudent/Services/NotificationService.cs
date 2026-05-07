using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TMS_SharedLibrary.Models;

namespace TMSStudent.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;
    private readonly IUserAPIService _userApiService;

    public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger, IUserAPIService userApiService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _userApiService = userApiService;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userOid)
    {
        try
        {
            // Get user ID from OID
            var user = await _userApiService.GetUserByOidAsync(userOid);
            if (user == null) 
            {
                _logger.LogWarning("User not found for OID: {Oid}", userOid);
                return new List<Notification>();
            }

            // Get notifications for this user
            var response = await _httpClient.GetAsync($"Notifications/ByRecipient/{user.UserId}");
            response.EnsureSuccessStatusCode();
            
            var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>() 
                ?? new List<Notification>();
            
            return notifications.OrderByDescending(n => n.DateSent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user OID: {Oid}", userOid);
            return new List<Notification>();
        }
    }

    public async Task<int> GetUnreadCountAsync(string userOid)
    {
        try
        {
            var notifications = await GetUserNotificationsAsync(userOid);
            return notifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification count for user OID: {Oid}", userOid);
            return 0;
        }
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"Notifications/{notificationId}");
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted notification {NotificationId}", notificationId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to delete notification {NotificationId}. Status: {StatusCode}", 
                    notificationId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<bool> DeleteAllNotificationsAsync(string userOid)
    {
        try
        {
            // Get user ID from OID
            var user = await _userApiService.GetUserByOidAsync(userOid);
            if (user == null) 
            {
                _logger.LogWarning("User not found for OID: {Oid}", userOid);
                return false;
            }

            // Get all notifications for this user
            var notifications = await GetUserNotificationsAsync(userOid);
            
            // Delete each notification
            var allDeleted = true;
            foreach (var notification in notifications)
            {
                var result = await DeleteNotificationAsync(notification.NotificationId);
                if (!result)
                {
                    allDeleted = false;
                }
            }

            if (allDeleted)
            {
                _logger.LogInformation("Successfully deleted all notifications for user {UserId}", user.UserId);
            }
            else
            {
                _logger.LogWarning("Some notifications failed to delete for user {UserId}", user.UserId);
            }

            return allDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all notifications for user OID: {Oid}", userOid);
            return false;
        }
    }
}
