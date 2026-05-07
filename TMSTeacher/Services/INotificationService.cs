using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services;

public interface INotificationService
{
    Task<List<Notification>> GetUserNotificationsAsync(string userOid);
    Task<int> GetUnreadCountAsync(string userOid);
    Task<bool> DeleteNotificationAsync(int notificationId);
    Task<bool> DeleteAllNotificationsAsync(string userOid);
}
