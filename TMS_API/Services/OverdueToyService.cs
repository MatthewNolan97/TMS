using Microsoft.EntityFrameworkCore;
using TMS_API.DAL;
using TMS_SharedLibrary.Models;

namespace TMS_API.Services;

public class OverdueToyService : IOverdueToyService
{
    private readonly K40TmsDdContext _context;
    private readonly INotificationRepo _notificationRepo;
    private readonly ILogger<OverdueToyService> _logger;

    public OverdueToyService(K40TmsDdContext context, INotificationRepo notificationRepo, ILogger<OverdueToyService> logger)
    {
        _context = context;
        _notificationRepo = notificationRepo;
        _logger = logger;
    }

    public async Task CheckAndSendOverdueNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting overdue toy check at {time}", DateTime.Now);

            // Get all overdue toy loans (not returned and due date is in the past)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var overdueLoans = await _context.ToyLoans
                .Include(tl => tl.Student)
                .Include(tl => tl.Toy)
                .Include(tl => tl.Student.User)
                .Where(tl => tl.ReturnDate == null && tl.DueDate < today)
                .ToListAsync();

            if (overdueLoans.Any())
            {
                _logger.LogInformation("Found {count} overdue toy loans", overdueLoans.Count);
                
                // Send notifications to students
                await SendStudentNotificationsAsync(overdueLoans);
                
                // Send notifications to teachers
                await SendTeacherNotificationsAsync(overdueLoans);
            }
            else
            {
                _logger.LogInformation("No overdue toy loans found");
            }

            // Send early warning notifications for toys due soon (within 2 days)
            await SendEarlyWarningNotificationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking overdue toys");
        }
    }

    public async Task SendEarlyWarningNotificationsAsync()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(2);
            
            var upcomingDueLoans = await _context.ToyLoans
                .Include(tl => tl.Student)
                .Include(tl => tl.Toy)
                .Include(tl => tl.Student.User)
                .Where(tl => tl.ReturnDate == null && 
                              tl.DueDate >= today && 
                              tl.DueDate <= warningDate)
                .ToListAsync();

            foreach (var loan in upcomingDueLoans)
            {
                var daysUntilDue = loan.DueDate.DayNumber - today.DayNumber;
                var message = daysUntilDue == 0 
                    ? $"Reminder: Your toy '{loan.Toy.Name}' is due today! Please return it to avoid late fees."
                    : $"Reminder: Your toy '{loan.Toy.Name}' is due in {daysUntilDue} day(s). Please return it soon.";

                var notification = new Notification
                {
                    RecipientId = loan.Student.UserId,
                    Message = message,
                    Type = "EarlyWarning",
                    DateSent = DateTime.Now
                };

                _notificationRepo.Create(notification);
            }

            _context.SaveChanges();
            
            if (upcomingDueLoans.Any())
            {
                _logger.LogInformation("Sent early warning notifications for {count} toys due soon", upcomingDueLoans.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending early warning notifications");
        }
    }

    public async Task SendTeacherNotificationsAsync(List<ToyLoan> overdueLoans)
    {
        try
        {
            // Get all teachers
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .ToListAsync();

            foreach (var teacher in teachers)
            {
                var message = $"Overdue Toys Alert: {overdueLoans.Count} toy(s) are overdue. " +
                             $"Please follow up with the respective students.";

                var notification = new Notification
                {
                    RecipientId = teacher.UserId,
                    Message = message,
                    Type = "OverdueAlert",
                    DateSent = DateTime.Now
                };

                _notificationRepo.Create(notification);
            }

            _context.SaveChanges();
            _logger.LogInformation("Sent overdue notifications to {count} teachers", teachers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending teacher notifications");
        }
    }

    public async Task SendStudentNotificationsAsync(List<ToyLoan> overdueLoans)
    {
        try
        {
            // Group overdue loans by student
            var studentOverdueLoans = overdueLoans.GroupBy(loan => loan.StudentId);

            foreach (var studentGroup in studentOverdueLoans)
            {
                var student = studentGroup.First().Student;
                var overdueToys = studentGroup.Select(loan => loan.Toy.Name).ToList();
                var toysList = string.Join(", ", overdueToys);
                
                var message = $"Your toy(s) are overdue: {toysList}. " +
                             $"Please return them as soon as possible to avoid additional consequences.";

                var notification = new Notification
                {
                    RecipientId = student.UserId,
                    Message = message,
                    Type = "Overdue",
                    DateSent = DateTime.Now
                };

                _notificationRepo.Create(notification);
            }

            _context.SaveChanges();
            _logger.LogInformation("Sent overdue notifications to {count} students", studentOverdueLoans.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending student notifications");
        }
    }
}
