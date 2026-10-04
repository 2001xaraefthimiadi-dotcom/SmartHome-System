using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartHome.API.Hubs;
using SmartHome.Core.Enums;
using SmartHome.Infrastructure.Data;

namespace SmartHome.API.Services
{
    public class AutomationSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<SmartHomeHub> _hubContext;
        private readonly ILogger<AutomationSchedulerService> _logger;

        public AutomationSchedulerService(
            IServiceScopeFactory scopeFactory,
            IHubContext<SmartHomeHub> hubContext,
            ILogger<AutomationSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckScheduledAutomationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Σφάλμα κατά τον έλεγχο των προγραμματισμένων αυτοματισμών.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task CheckScheduledAutomationsAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            var automations = await context.Automations
                .Include(a => a.TargetDevice)
                .Where(a =>
                    a.IsActive &&
                    a.ConditionType == ConditionType.TimeOfDay &&
                    a.ScheduledTime.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var automation in automations)
            {
                // Έχει ήδη εκτελεστεί σήμερα;
                if (automation.LastExecutedAt.HasValue &&
                    automation.LastExecutedAt.Value.Date == now.Date)
                {
                    continue;
                }

                var scheduledTime = automation.ScheduledTime!.Value;

                // Εκτέλεση όταν φτάσουμε στην προγραμματισμένη ώρα.
                // Το παράθυρο των 30 sec ταιριάζει με το polling του service.
                if (currentTime < scheduledTime ||
                    currentTime >= scheduledTime.Add(TimeSpan.FromSeconds(30)))
                {
                    continue;
                }

                switch (automation.Action)
                {
                    case ActionType.TurnOn:
                        automation.TargetDevice.Status = DeviceStatus.On;
                        break;

                    case ActionType.TurnOff:
                        automation.TargetDevice.Status = DeviceStatus.Off;
                        break;

                    case ActionType.Toggle:
                        automation.TargetDevice.Status =
                            automation.TargetDevice.Status == DeviceStatus.On
                                ? DeviceStatus.Off
                                : DeviceStatus.On;
                        break;

                    default:
                        continue;
                }

                automation.LastExecutedAt = now;

                await context.SaveChangesAsync(cancellationToken);

                var statusText =
                    automation.TargetDevice.Status == DeviceStatus.On
                        ? "ΕΝΕΡΓΟΠΟΙΗΘΗΚΕ"
                        : "ΑΠΕΝΕΡΓΟΠΟΙΗΘΗΚΕ";

                var notificationMessage =
                    $"{automation.Name}\n" +
                    $"{automation.TargetDevice.Name}: {statusText}.";

                await _hubContext.Clients.All.SendAsync(
                    "AutomationExecuted",
                    notificationMessage,
                    cancellationToken);

                await _hubContext.Clients.All.SendAsync(
                    "DeviceStatusChanged",
                    automation.TargetDevice.Id,
                    cancellationToken);

                _logger.LogInformation(
                    "Εκτελέστηκε ο προγραμματισμένος αυτοματισμός {AutomationId}: {AutomationName}",
                    automation.Id,
                    automation.Name);
            }
        }
    }
}