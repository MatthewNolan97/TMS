using TMS_SharedLibrary.Models;

namespace TMS_API.DAL;

public interface INotificationRepo
{
    IEnumerable<Notification> GetAll();
    Notification? FindById(int id);
    IEnumerable<Notification> GetByRecipientId(int recipientId);
    Notification Create(Notification notification);
    void Update(Notification notification);
    void Delete(int id);
}
