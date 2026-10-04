using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SmartHome.Blazor.Models;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;

namespace SmartHome.Blazor.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        private readonly IJSRuntime _js;

        private HubConnection? _hubConnection;

        public static string? Token { get; set; }

        public ApiService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }


        private async Task AddAuthHeader()
        {
            await LoadTokenAsync();

            if (!string.IsNullOrEmpty(Token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);
            }
        }

        public async Task<string?> Login(string email, string password)
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

            var response = await _http.PostAsync("/api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            if (!string.IsNullOrEmpty(token))
            {
                await SaveTokenAsync(token);
            }

            return token;
        }

        public async Task<string> GetDevices()
        {
            await AddAuthHeader();
            return await _http.GetStringAsync("/api/Device");
        }
        public async Task<List<DeviceModel>?> GetDevicesList()
        {
            await AddAuthHeader();
            return await _http.GetFromJsonAsync<List<DeviceModel>>("/api/Device");
        }
        public async Task<bool> CreateDevice(DeviceModel device)
        {
            await AddAuthHeader();
            var response = await _http.PostAsJsonAsync("/api/Device", new
            {
                name = device.Name,
                type = device.Type,
                location = device.Location,
                powerConsumption = device.PowerConsumption
            });

            return response.IsSuccessStatusCode;
        }
        public async Task<List<AutomationModel>?> GetAutomations()
        {
            await AddAuthHeader();
            return await _http.GetFromJsonAsync<List<AutomationModel>>("/api/Automation");
        }

        public async Task<bool> CreateAutomation(AutomationModel automation)
        {
            await AddAuthHeader();

            var response = await _http.PostAsJsonAsync("/api/Automation", new
            {
                name = automation.Name,
                conditionType = automation.ConditionType,
                conditionValue = automation.ConditionValue,

                sourceDeviceId = automation.ConditionType == 4
                    ? null
                    : automation.SourceDeviceId,

                targetDeviceId = automation.TargetDeviceId,
                action = automation.Action,

                scheduledTime = automation.ConditionType == 4 &&
                                automation.ScheduledTime.HasValue
                    ? automation.ScheduledTime.Value.ToString("HH:mm:ss")
                    : null
            });

            return response.IsSuccessStatusCode;
        }
        public async Task<string> SendReading(ReadingModel reading)
        {
            await AddAuthHeader();

            var response = await _http.PostAsJsonAsync("/api/DeviceReadings", new
            {
                deviceId = reading.DeviceId,
                readingType = reading.ReadingType,
                value = reading.Value
            });

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"ERROR {(int)response.StatusCode}: {content}";
            }

            return content;
        }
        public async Task<List<EnergyLogModel>?> GetEnergyLogs(int deviceId)
        {
            await AddAuthHeader();
            return await _http.GetFromJsonAsync<List<EnergyLogModel>>
                ($"/api/EnergyLogs/device/{deviceId}");
        }

        public async Task<TotalEnergyModel?> GetTotalEnergy(int deviceId)
        {
            await AddAuthHeader();
            return await _http.GetFromJsonAsync<TotalEnergyModel>
                ($"/api/EnergyLogs/device/{deviceId}/total");
        }
        public async Task SaveTokenAsync(string token)
        {
            Token = token;
            await _js.InvokeVoidAsync("authStorage.setToken", token);
        }

        public async Task LoadTokenAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                Token = await _js.InvokeAsync<string?>("authStorage.getToken");
            }
        }

        public async Task LogoutAsync()
        {
            Token = null;
            _http.DefaultRequestHeaders.Authorization = null;
            await _js.InvokeVoidAsync("authStorage.removeToken");
        }
        public async Task StartSignalRAsync(
            Func<Task> onDeviceStatusChanged,
            Func<string, Task>? onAutomationExecuted = null)
        {
            if (_hubConnection != null)
            {
                _hubConnection.On("DeviceStatusChanged", async () =>
                {
                    await onDeviceStatusChanged();
                });

                if (onAutomationExecuted != null)
                {
                    _hubConnection.On<string>("AutomationExecuted", async (message) =>
                    {
                        await onAutomationExecuted(message);
                    });
                }

                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7082/smartHomeHub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("DeviceStatusChanged", async () =>
            {
                await onDeviceStatusChanged();
            });

            if (onAutomationExecuted != null)
            {
                _hubConnection.On<string>("AutomationExecuted", async (message) =>
                {
                    await onAutomationExecuted(message);
                });
            }

            await _hubConnection.StartAsync();
        }
        public async Task<bool> UpdateDevice(DeviceModel device)
        {
            await AddAuthHeader();

            var response = await _http.PutAsJsonAsync($"/api/Device/{device.Id}", new
            {
                name = device.Name,
                type = device.Type,
                location = device.Location,
                status = device.Status,
                isOnline = device.IsOnline,
                powerConsumption = device.PowerConsumption
            });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDevice(int id)
        {
            await AddAuthHeader();

            var response = await _http.DeleteAsync($"/api/Device/{id}");

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateAutomation(AutomationModel automation)
        {
            await AddAuthHeader();

            var response = await _http.PutAsJsonAsync(
                $"/api/Automation/{automation.Id}",
                new
                {
                    name = automation.Name,
                    conditionType = automation.ConditionType,
                    conditionValue = automation.ConditionValue,

                    sourceDeviceId = automation.ConditionType == 4
                        ? null
                        : automation.SourceDeviceId,

                    targetDeviceId = automation.TargetDeviceId,
                    action = automation.Action,
                    isActive = automation.IsActive,

                    scheduledTime = automation.ConditionType == 4 &&
                                    automation.ScheduledTime.HasValue
                        ? automation.ScheduledTime.Value.ToString("HH:mm:ss")
                        : null
                });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAutomation(int id)
        {
            await AddAuthHeader();

            var response = await _http.DeleteAsync($"/api/Automation/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}