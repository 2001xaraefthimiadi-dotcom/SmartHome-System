using System.Net.Http.Headers;
using System.Text.Json;
using SmartHome.Mobile.Models;

namespace SmartHome.Mobile;

public partial class EnergyPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    private List<DeviceModel> _devices = [];

    private readonly EnergyChartDrawable _chartDrawable = new();

    public EnergyPage()
    {
        InitializeComponent();

        EnergyChartView.Drawable = _chartDrawable;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDevicesAsync();
    }

    private async Task LoadDevicesAsync()
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

            var response =
                await _httpClient.GetAsync($"{ApiBaseUrl}/api/Device");

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Failed to load devices.";
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            _devices =
                JsonSerializer.Deserialize<List<DeviceModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? [];

            DevicePicker.ItemsSource = _devices;

            if (_devices.Count > 0 && DevicePicker.SelectedIndex < 0)
            {
                DevicePicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
        }
    }

    private async void OnDeviceSelected(object? sender, EventArgs e)
    {
        if (DevicePicker.SelectedItem is DeviceModel)
        {
            await LoadEnergyDataAsync();
        }
    }

    private async void OnLoadEnergyClicked(object? sender, EventArgs e)
    {
        await LoadEnergyDataAsync();
    }

    private async Task LoadEnergyDataAsync()
    {
        try
        {
            MessageLabel.Text = string.Empty;

            if (DevicePicker.SelectedItem is not DeviceModel selectedDevice)
            {
                MessageLabel.Text = "Please select a device.";
                return;
            }

            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageLabel.Text = "You are not authenticated.";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var logsTask = _httpClient.GetAsync(
                $"{ApiBaseUrl}/api/EnergyLogs/device/{selectedDevice.Id}");

            var totalTask = _httpClient.GetAsync(
                $"{ApiBaseUrl}/api/EnergyLogs/device/{selectedDevice.Id}/total");

            await Task.WhenAll(logsTask, totalTask);

            var logsResponse = await logsTask;
            var totalResponse = await totalTask;

            if (!logsResponse.IsSuccessStatusCode ||
                !totalResponse.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Failed to load energy data.";
                return;
            }

            var logsJson =
                await logsResponse.Content.ReadAsStringAsync();

            var totalJson =
                await totalResponse.Content.ReadAsStringAsync();

            var logs =
                JsonSerializer.Deserialize<List<EnergyLogModel>>(
                    logsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? [];

            var total =
                JsonSerializer.Deserialize<TotalEnergyModel>(
                    totalJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            TotalEnergyLabel.Text =
                $"{total?.TotalConsumedWatts ?? 0:N1} W";

            EnergyLogsCollection.ItemsSource =
                logs.Select(log => new
                {
                    WattsText = $"{log.ConsumedWatts:N1} W",
                    DateText = log.RecordedAt
                        .ToLocalTime()
                        .ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            _chartDrawable.Logs = logs;

            EnergyChartView.Invalidate();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
        }
    }
}