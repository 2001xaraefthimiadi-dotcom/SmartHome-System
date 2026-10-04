using SmartHome.Mobile.Models;
using SmartHome.Mobile.ViewModels;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartHome.Mobile;

public partial class DevicesPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    public DevicesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDevices();
    }

    private async Task LoadDevices()
    {
        try
        {
            MessageLabel.Text = "";

            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageLabel.Text = "Δεν έχετε συνδεθεί.";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Device");

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Αποτυχία φόρτωσης των συσκευών.";
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var devices = JsonSerializer.Deserialize<List<DeviceModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var viewModels = devices?
                .Select(d => new DeviceViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Location = d.Location,
                    Type = d.Type,
                    Status = d.Status,
                    IsOnline = d.IsOnline,
                    PowerConsumption = d.PowerConsumption
                })
                .ToList();

            DevicesCollection.ItemsSource = viewModels;
        }
        catch (Exception)
        {
            MessageLabel.Text = "Παρουσιάστηκε σφάλμα κατά τη φόρτωση των συσκευών.";
        }
    }
    private async void OnDeviceTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not DeviceViewModel device)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(DeviceDetailsPage)}?deviceId={device.Id}");
    }
    private async void OnRefresh(object? sender, EventArgs e)
    {
        try
        {
            await LoadDevices();
        }
        finally
        {
            DevicesRefreshView.IsRefreshing = false;
        }
    }
    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            await LoadDevices();
        }
        catch (Exception)
        {
            MessageLabel.Text = "Παρουσιάστηκε σφάλμα κατά την ανανέωση των συσκευών.";
        }
    }
}