using System.Text;
using System.Text.Json;

namespace SmartHome.Mobile;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private const string ApiBaseUrl = "https://localhost:7082";

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        MessageLabel.Text = string.Empty;
        MessageLabel.TextColor = Colors.Red;

        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            MessageLabel.Text = "Παρακαλώ εισαγάγετε email και κωδικό πρόσβασης.";
            return;
        }

        try
        {
            var payload = new
            {
                email,
                password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"{ApiBaseUrl}/api/Auth/login",
                content);

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Η σύνδεση απέτυχε. Ελέγξτε τα στοιχεία σας.";
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var token = doc.RootElement
                .GetProperty("token")
                .GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageLabel.Text = "Μη έγκυρη απόκριση σύνδεσης.";
                return;
            }

            await SecureStorage.SetAsync("jwt_token", token);

            MessageLabel.TextColor = Colors.Green;
            MessageLabel.Text = "Η σύνδεση πραγματοποιήθηκε επιτυχώς!";

            await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
        }
        catch (Exception)
        {
            MessageLabel.TextColor = Colors.Red;
            MessageLabel.Text = "Παρουσιάστηκε σφάλμα κατά τη σύνδεση. Προσπαθήστε ξανά.";
        }
    }
}