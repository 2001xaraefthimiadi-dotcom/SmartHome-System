using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHome.Mobile.Services;

public class SignalRService
{
    private HubConnection? _connection;

    public async Task StartAsync(Func<string, Task> onAutomationExecuted)
    {
        if (_connection is not null)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7082/smartHomeHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>(
            "AutomationExecuted",
            async message =>
            {
                await onAutomationExecuted(message);
            });

        await _connection.StartAsync();
    }
}