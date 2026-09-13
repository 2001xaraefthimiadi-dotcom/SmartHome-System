using System.Net.Http.Headers;
using System.Text.Json;
using SmartHome.Mobile.Models;
using SmartHome.Mobile.Services;

namespace SmartHome.Mobile;

public partial class DashboardPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    private readonly SignalRService _signalRService;

    public DashboardPage(SignalRService signalRService)
    {
        InitializeComponent();
        _signalRService = signalRService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDashboardStats();

        await _signalRService.StartAsync(async message =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlertAsync(
                    "Automation Executed",
                    message,
                    "OK");
            });
        
        });
    }
    private async Task LoadDashboardStats()
    {
        try
        {
            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
                return;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var devicesResponse =
                await _httpClient.GetAsync($"{ApiBaseUrl}/api/Device");

            var automationsResponse =
                await _httpClient.GetAsync($"{ApiBaseUrl}/api/Automation");

            if (!devicesResponse.IsSuccessStatusCode ||
                !automationsResponse.IsSuccessStatusCode)
                return;

            var devicesJson =
                await devicesResponse.Content.ReadAsStringAsync();

            var automationsJson =
                await automationsResponse.Content.ReadAsStringAsync();

            var devices =
                JsonSerializer.Deserialize<List<DeviceModel>>(
                    devicesJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            var automations =
                JsonSerializer.Deserialize<List<AutomationModel>>(
                    automationsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            TotalDevicesLabel.Text =
                $"Total Devices: {devices?.Count ?? 0}";

            OnlineDevicesLabel.Text =
                $"Online Devices: {devices?.Count(d => d.IsOnline) ?? 0}";

            PoweredOnLabel.Text =
                $"Powered On: {devices?.Count(d => d.Status == 1) ?? 0}";

            ActiveAutomationsLabel.Text =
                $"Active Automations: {automations?.Count(a => a.IsActive) ?? 0}";
        }
        catch
        {
        }
    }
}
