using System.Timers;

namespace TMS_API.Services;

public class BackgroundSchedulerService : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundSchedulerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly System.Timers.Timer _timer;
    private bool _disposed = false;

    public BackgroundSchedulerService(ILogger<BackgroundSchedulerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _timer = new System.Timers.Timer();
        _timer.Elapsed += OnTimerElapsed;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background Scheduler Service starting...");
        
        // Calculate time, set as HOUR/MINUTE/SECOND
        var now = DateTime.Now;
        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
        
        // If it's already past 6 AM, schedule for tomorrow
        if (now > scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }
        
        var initialDelay = scheduledTime - now;
        _logger.LogInformation("Scheduling first overdue toy check for {time}", scheduledTime);
        
        // Set initial delay
        _timer.Interval = initialDelay.TotalMilliseconds;
        _timer.AutoReset = false; // Don't auto-reset initially
        _timer.Start();
        
        return Task.CompletedTask;
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _logger.LogInformation("Running scheduled overdue toy check at {time}", DateTime.Now);
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var overdueToyService = scope.ServiceProvider.GetRequiredService<IOverdueToyService>();
                await overdueToyService.CheckAndSendOverdueNotificationsAsync();
            }
            
            _logger.LogInformation("Completed scheduled overdue toy check");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during scheduled overdue toy check");
        }
        finally
        {
            // Reset timer to run every 24 hours
            _timer.Interval = TimeSpan.FromHours(24).TotalMilliseconds;
            _timer.AutoReset = true;
            _timer.Start();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background Scheduler Service stopping...");
        _timer?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer?.Dispose();
            _disposed = true;
        }
    }
}
