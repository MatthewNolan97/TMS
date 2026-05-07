using TMS_SharedLibrary.Models;

namespace TMS_API.DAL;

public class NotificationRepo : INotificationRepo
{
    private readonly K40TmsDdContext _context;

    public NotificationRepo(K40TmsDdContext context)
    {
        _context = context;
    }

    public IEnumerable<Notification> GetAll()
    {
        return _context.Notifications.ToList();
    }

    public Notification? FindById(int id)
    {
        return _context.Notifications.Find(id);
    }

    public IEnumerable<Notification> GetByRecipientId(int recipientId)
    {
        return _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.DateSent)
            .ToList();
    }

    public Notification Create(Notification notification)
    {
        _context.Notifications.Add(notification);
        _context.SaveChanges();
        return notification;
    }

    public void Update(Notification notification)
    {
        _context.Notifications.Update(notification);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var notification = _context.Notifications.Find(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            _context.SaveChanges();
        }
    }
}
