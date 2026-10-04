using SmartHome.Mobile.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartHome.Mobile;

[QueryProperty(nameof(AutomationId), "automationId")]
public partial class EditAutomationPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl =
        "https://localhost:7082";

    private List<DevicePickerItem> _devices = new();

    private int _automationId;

    private bool _loaded;

    public string AutomationId
    {
        set
        {
            if (int.TryParse(value, out var id))
                _automationId = id;
        }
    }

    public EditAutomationPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_loaded)
            return;

        await LoadData();

        _loaded = true;
    }

    private async Task LoadData()
    {
        try
        {
            MessageLabel.Text = "";

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

            await LoadDevices();

            await LoadAutomation();
        }
        catch (Exception)
        {
            ShowError(
                "Παρουσιάστηκε σφάλμα κατά τη φόρτωση του αυτοματισμού.");
        }
    }

    private async Task LoadDevices()
    {
        var response = await _httpClient.GetAsync(
            $"{ApiBaseUrl}/api/Device");

        if (!response.IsSuccessStatusCode)
            throw new Exception();

        var json =
            await response.Content.ReadAsStringAsync();

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
                DisplayName =
                    $"{d.Name} ({d.Location})"
            })
            .ToList()
            ?? new List<DevicePickerItem>();

        SourceDevicePicker.ItemsSource = _devices;
        TargetDevicePicker.ItemsSource = _devices;
    }

    private async Task LoadAutomation()
    {
        var response = await _httpClient.GetAsync(
            $"{ApiBaseUrl}/api/Automation/{_automationId}");

        if (!response.IsSuccessStatusCode)
        {
            ShowError(
                "Αποτυχία φόρτωσης του αυτοματισμού.");

            return;
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var automation =
            JsonSerializer.Deserialize<AutomationModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (automation is null)
        {
            ShowError(
                "Δεν βρέθηκε ο αυτοματισμός.");

            return;
        }

        NameEntry.Text = automation.Name;

        ConditionPicker.SelectedIndex =
            automation.ConditionType - 1;

        ConditionValueEntry.Text =
            automation.ConditionValue.ToString(
                CultureInfo.CurrentCulture);

        ActionPicker.SelectedIndex =
            automation.Action - 1;

        ActiveSwitch.IsToggled =
            automation.IsActive;

        SourceDevicePicker.SelectedItem =
            _devices.FirstOrDefault(
                d => d.Id == automation.SourceDeviceId);

        TargetDevicePicker.SelectedItem =
            _devices.FirstOrDefault(
                d => d.Id == automation.TargetDeviceId);

        if (automation.ScheduledTime.HasValue)
        {
            ScheduledTimePicker.Time =
                automation.ScheduledTime.Value;
        }

        UpdateConditionVisibility();
    }

    private void OnConditionChanged(
        object? sender,
        EventArgs e)
    {
        UpdateConditionVisibility();
    }

    private void UpdateConditionVisibility()
    {
        var conditionType =
            ConditionPicker.SelectedIndex + 1;

        var isTimeAutomation =
            conditionType == 4;

        SourceDeviceSection.IsVisible =
            conditionType > 0 &&
            !isTimeAutomation;

        ConditionValueSection.IsVisible =
            conditionType > 0 &&
            !isTimeAutomation;

        ScheduledTimeSection.IsVisible =
            isTimeAutomation;

        if (isTimeAutomation)
        {
            SourceDevicePicker.SelectedItem = null;
            ConditionValueEntry.Text = "";
        }
    }

    private async void OnSaveClicked(
        object? sender,
        EventArgs e)
    {
        MessageLabel.Text = "";

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError(
                "Παρακαλώ εισάγετε όνομα αυτοματισμού.");

            return;
        }

        if (ConditionPicker.SelectedIndex < 0)
        {
            ShowError(
                "Παρακαλώ επιλέξτε τύπο συνθήκης.");

            return;
        }

        var conditionType =
            ConditionPicker.SelectedIndex + 1;

        var isTimeAutomation =
            conditionType == 4;

        DevicePickerItem? sourceDevice =
            SourceDevicePicker.SelectedItem
                as DevicePickerItem;

        if (!isTimeAutomation &&
            sourceDevice is null)
        {
            ShowError(
                "Παρακαλώ επιλέξτε συσκευή πηγής.");

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
            ShowError(
                "Παρακαλώ εισάγετε έγκυρη τιμή συνθήκης.");

            return;
        }

        if (TargetDevicePicker.SelectedItem
            is not DevicePickerItem targetDevice)
        {
            ShowError(
                "Παρακαλώ επιλέξτε συσκευή στόχου.");

            return;
        }

        if (ActionPicker.SelectedIndex < 0)
        {
            ShowError(
                "Παρακαλώ επιλέξτε ενέργεια.");

            return;
        }

        var request = new
        {
            name = NameEntry.Text.Trim(),

            conditionType,

            conditionValue,

            sourceDeviceId = isTimeAutomation
                ? (int?)null
                : sourceDevice?.Id,

            targetDeviceId =
                targetDevice.Id,

            action =
                ActionPicker.SelectedIndex + 1,

            isActive =
                ActiveSwitch.IsToggled,

            scheduledTime =
                isTimeAutomation &&
                ScheduledTimePicker.Time.HasValue
                    ? $"{ScheduledTimePicker.Time.Value.Hours:D2}:{ScheduledTimePicker.Time.Value.Minutes:D2}:00"
                    : null
        };

        try
        {
            var response =
                await _httpClient.PutAsJsonAsync(
                    $"{ApiBaseUrl}/api/Automation/{_automationId}",
                    request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                ShowError(
                    $"Αποτυχία ενημέρωσης του αυτοματισμού. {error}");

                return;
            }

            MessageLabel.TextColor =
                Colors.Green;

            MessageLabel.Text =
                "Ο αυτοματισμός ενημερώθηκε επιτυχώς.";

            await Task.Delay(700);

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception)
        {
            ShowError(
                "Παρουσιάστηκε σφάλμα κατά την ενημέρωση του αυτοματισμού.");
        }
    }

    private void ShowError(string message)
    {
        MessageLabel.TextColor =
            Colors.Red;

        MessageLabel.Text =
            message;
    }

    private class DevicePickerItem
    {
        public int Id { get; set; }

        public string DisplayName { get; set; } = "";
    }
}