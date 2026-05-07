using TMS_SharedLibrary.Models;

namespace TMS_API.Services;

public interface IOverdueToyService
{
    Task CheckAndSendOverdueNotificationsAsync();
    Task SendEarlyWarningNotificationsAsync();
    Task SendTeacherNotificationsAsync(List<ToyLoan> overdueLoans);
    Task SendStudentNotificationsAsync(List<ToyLoan> overdueLoans);
}
