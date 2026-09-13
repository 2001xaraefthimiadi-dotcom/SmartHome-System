using SmartHome.Mobile.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartHome.Mobile;

[QueryProperty(nameof(DeviceId), "deviceId")]
public partial class DeviceDetailsPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    private int _deviceId;

    public int DeviceId
    {
        get => _deviceId;
        set
        {
            _deviceId = value;

            if (_deviceId > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadDeviceDetailsAsync();
                });
            }
        }
    }

    public DeviceDetailsPage()
    {
        InitializeComponent();
    }

    private async Task LoadDeviceDetailsAsync()
    {
        try
        {
            MessageLabel.Text = string.Empty;

            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageLabel.Text = "You are not authenticated.";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var devicesResponse =
                await _httpClient.GetAsync($"{ApiBaseUrl}/api/Device");

            var energyResponse =
                await _httpClient.GetAsync(
                    $"{ApiBaseUrl}/api/EnergyLogs/device/{DeviceId}");

            if (!devicesResponse.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Failed to load device.";
                return;
            }

            var devicesJson =
                await devicesResponse.Content.ReadAsStringAsync();

            var devices =
                JsonSerializer.Deserialize<List<DeviceModel>>(
                    devicesJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? [];

            var device = devices.FirstOrDefault(d => d.Id == DeviceId);

            if (device is null)
            {
                MessageLabel.Text = "Device not found.";
                return;
            }

            DeviceNameLabel.Text = device.Name;
            LocationLabel.Text = $"📍 {device.Location}";
            StatusLabel.Text = device.Status == 1 ? "ON" : "OFF";
            OnlineLabel.Text = device.IsOnline ? "Online" : "Offline";
            DeviceTypeLabel.Text = GetDeviceType(device.Type);
            DeviceIconLabel.Text = GetDeviceIcon(device.Type);
            PowerLabel.Text = $"{device.PowerConsumption:N1} W";

            StatusLabel.TextColor =
                device.Status == 1 ? Colors.Green : Colors.Gray;

            OnlineLabel.TextColor =
                device.IsOnline ? Colors.Green : Colors.Red;

            if (!energyResponse.IsSuccessStatusCode)
            {
                EnergyLogsCollection.ItemsSource = null;
                return;
            }

            var energyJson =
                await energyResponse.Content.ReadAsStringAsync();

            var energyLogs =
                JsonSerializer.Deserialize<List<EnergyLogModel>>(
                    energyJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? [];

            EnergyLogsCollection.ItemsSource =
                energyLogs.Take(5)
                    .Select(log => new
                    {
                        WattsText = $"{log.ConsumedWatts:N1} W",
                        DateText = log.RecordedAt
                            .ToLocalTime()
                            .ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToList();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
        }
    }

    private static string GetDeviceType(int type) => type switch
    {
        1 => "Light",
        2 => "Thermostat",
        3 => "Sensor",
        4 => "Smart Plug",
        5 => "Door Lock",
        _ => "Unknown"
    };

    private static string GetDeviceIcon(int type) => type switch
    {
        1 => "💡",
        2 => "🌡️",
        3 => "📡",
        4 => "🔌",
        5 => "🚪",
        _ => "🏠"
    };
}