using SmartHome.Mobile.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartHome.Mobile;

public partial class AutomationsPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    public AutomationsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAutomations();
    }

    private async Task LoadAutomations()
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

            var response = await _httpClient.GetAsync(
                $"{ApiBaseUrl}/api/Automation");

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Αποτυχία φόρτωσης των αυτοματισμών.";
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var automations = JsonSerializer.Deserialize<List<AutomationModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var viewModels = automations?.Select(a => new
            {
                a.Id,
                a.Name,

                ConditionText = a.ConditionType == 4
                    ? $"Ώρα εκτέλεσης: {a.ScheduledTime?.ToString(@"hh\:mm") ?? "-"}"
                    : $"{GetCondition(a.ConditionType)} {a.ConditionValue}",

                TargetDeviceText = !string.IsNullOrWhiteSpace(a.TargetDeviceName)
                    ? $"Συσκευή στόχου: {a.TargetDeviceName}"
                    : $"Συσκευή στόχου: #{a.TargetDeviceId}",

                ActionText = GetAction(a.Action),

                StatusText = a.IsActive
                    ? "● ΕΝΕΡΓΟΣ"
                    : "● ΑΝΕΝΕΡΓΟΣ"
            }).ToList();

            AutomationsCollection.ItemsSource = viewModels;
        }
        catch (Exception)
        {
            MessageLabel.Text =
                "Παρουσιάστηκε σφάλμα κατά τη φόρτωση των αυτοματισμών.";
        }
    }
    private async void OnCreateAutomationClicked(
    object? sender,
    EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(CreateAutomationPage));
    }
    private async void OnEditAutomationClicked(
    object? sender,
    EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not int automationId)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(EditAutomationPage)}?automationId={automationId}");
    }

    private async void OnDeleteAutomationClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not int automationId)
            return;

        bool confirm = await DisplayAlert(
            "Διαγραφή Αυτοματισμού",
            "Θέλετε σίγουρα να διαγράψετε αυτόν τον αυτοματισμό;",
            "Διαγραφή",
            "Ακύρωση");

        if (!confirm)
            return;

        try
        {
            var token = await SecureStorage.GetAsync("jwt_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                ShowMessage(
                    "Δεν έχετε συνδεθεί.",
                    Colors.Red);

                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response = await _httpClient.DeleteAsync(
                $"{ApiBaseUrl}/api/Automation/{automationId}");

            if (!response.IsSuccessStatusCode)
            {
                ShowMessage(
                    "Αποτυχία διαγραφής του αυτοματισμού.",
                    Colors.Red);

                return;
            }

            ShowMessage(
                "Ο αυτοματισμός διαγράφηκε επιτυχώς.",
                Colors.Green);

            await LoadAutomations();
        }
        catch (Exception)
        {
            ShowMessage(
                "Παρουσιάστηκε σφάλμα κατά τη διαγραφή του αυτοματισμού.",
                Colors.Red);
        }
    }

    private void ShowMessage(
        string message,
        Color color)
    {
        MessageLabel.TextColor = color;
        MessageLabel.Text = message;
    }

    private string GetCondition(int conditionType) => conditionType switch
    {
        1 => "Θερμοκρασία >",
        2 => "Θερμοκρασία <",
        3 => "Ενέργεια >",
        4 => "Ώρα εκτέλεσης",
        _ => "Άγνωστη συνθήκη"
    };

    private string GetAction(int action) => action switch
    {
        1 => "Ενεργοποίηση",
        2 => "Απενεργοποίηση",
        3 => "Εναλλαγή κατάστασης",
        _ => "Άγνωστη ενέργεια"
    };
}