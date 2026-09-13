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
                MessageLabel.Text = "Not authenticated.";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Automation");

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Failed to load automations.";
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
                ConditionText = $"{GetCondition(a.ConditionType)} {a.ConditionValue}",
                ActionText = GetAction(a.Action),
                StatusText = a.IsActive ? "● ACTIVE" : "● DISABLED"
            }).ToList();

            AutomationsCollection.ItemsSource = viewModels;
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
        }
    }

    private string GetCondition(int conditionType) => conditionType switch
    {
        1 => "Temperature >",
        2 => "Temperature <",
        3 => "Energy >",
        _ => "Unknown"
    };

    private string GetAction(int action) => action switch
    {
        1 => "Turn On",
        2 => "Turn Off",
        3 => "Toggle",
        _ => "Unknown"
    };
}