using SmartHome.Mobile.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartHome.Mobile;

public partial class CreateAutomationPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    private List<DevicePickerItem> _devices = new();

    public CreateAutomationPage()
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
            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageLabel.Text = "Δεν έχετε συνδεθεί.";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                $"{ApiBaseUrl}/api/Device");

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text =
                    "Αποτυχία φόρτωσης των συσκευών.";
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var devices =
                JsonSerializer.Deserialize<List<DeviceModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            _devices = devices?
                .Select(d => new DevicePickerItem
                {
                    Id = d.Id,
                    DisplayName = $"{d.Name} ({d.Location})"
                })
                .ToList()
                ?? new List<DevicePickerItem>();

            SourceDevicePicker.ItemsSource = _devices;
            TargetDevicePicker.ItemsSource = _devices;
        }
        catch (Exception)
        {
            MessageLabel.Text =
                "Παρουσιάστηκε σφάλμα κατά τη φόρτωση των συσκευών.";
        }
    }

    private void OnConditionChanged(
        object? sender,
        EventArgs e)
    {
        var conditionType = ConditionPicker.SelectedIndex + 1;

        if (conditionType <= 0)
        {
            SourceDeviceSection.IsVisible = false;
            ConditionValueSection.IsVisible = false;
            ScheduledTimeSection.IsVisible = false;
            return;
        }

        var isTimeAutomation = conditionType == 4;

        SourceDeviceSection.IsVisible = !isTimeAutomation;
        ConditionValueSection.IsVisible = !isTimeAutomation;
        ScheduledTimeSection.IsVisible = isTimeAutomation;

        if (isTimeAutomation)
        {
            SourceDevicePicker.SelectedItem = null;
            ConditionValueEntry.Text = "";
        }
    }

    private async void OnCreateClicked(
        object? sender,
        EventArgs e)
    {
        MessageLabel.Text = "";

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("Παρακαλώ εισάγετε όνομα αυτοματισμού.");
            return;
        }

        if (ConditionPicker.SelectedIndex < 0)
        {
            ShowError("Παρακαλώ επιλέξτε τύπο συνθήκης.");
            return;
        }

        var conditionType =
            ConditionPicker.SelectedIndex + 1;

        var isTimeAutomation =
            conditionType == 4;

        if (!isTimeAutomation &&
            SourceDevicePicker.SelectedItem is not DevicePickerItem)
        {
            ShowError("Παρακαλώ επιλέξτε συσκευή πηγής.");
            return;
        }

        double conditionValue = 0;

        if (!isTimeAutomation &&
            !double.TryParse(
                ConditionValueEntry.Text,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out conditionValue) &&
            !double.TryParse(
                ConditionValueEntry.Text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out conditionValue))
        {
            ShowError("Παρακαλώ εισάγετε έγκυρη τιμή συνθήκης.");
            return;
        }

        if (TargetDevicePicker.SelectedItem
            is not DevicePickerItem targetDevice)
        {
            ShowError("Παρακαλώ επιλέξτε συσκευή στόχου.");
            return;
        }

        if (ActionPicker.SelectedIndex < 0)
        {
            ShowError("Παρακαλώ επιλέξτε ενέργεια.");
            return;
        }

        DevicePickerItem? sourceDevice =
            SourceDevicePicker.SelectedItem
                as DevicePickerItem;

        var request = new
        {
            name = NameEntry.Text.Trim(),

            conditionType,

            conditionValue,

            sourceDeviceId = isTimeAutomation
                ? (int?)null
                : sourceDevice?.Id,

            targetDeviceId = targetDevice.Id,

            action = ActionPicker.SelectedIndex + 1,

            scheduledTime = isTimeAutomation &&
                ScheduledTimePicker.Time.HasValue
                ? $"{ScheduledTimePicker.Time.Value.Hours:D2}:{ScheduledTimePicker.Time.Value.Minutes:D2}:00"
                : null
        };

        try
        {
            var token =
                await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                ShowError("Δεν έχετε συνδεθεί.");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await _httpClient.PostAsJsonAsync(
                    $"{ApiBaseUrl}/api/Automation",
                    request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ShowError(
                    $"Αποτυχία δημιουργίας αυτοματισμού. {error}");

                return;
            }

            MessageLabel.TextColor =
                Colors.Green;

            MessageLabel.Text =
                "Ο αυτοματισμός δημιουργήθηκε επιτυχώς.";

            await Task.Delay(800);

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception)
        {
            ShowError(
                "Παρουσιάστηκε σφάλμα κατά τη δημιουργία του αυτοματισμού.");
        }
    }

    private void ShowError(string message)
    {
        MessageLabel.TextColor = Colors.Red;
        MessageLabel.Text = message;
    }

    private class DevicePickerItem
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = "";
    }
}