namespace SmartHome.Mobile.ViewModels;

public class DeviceViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Type { get; set; }

    public int Status { get; set; }

    public bool IsOnline { get; set; }

    public double PowerConsumption { get; set; }

    public string StatusText =>
        Status == 1 ? "● ON" : "● OFF";

    public string OnlineText =>
        IsOnline ? "Online" : "Offline";

    public string PowerText =>
        $"{PowerConsumption:N1} W";

    public string TypeText => Type switch
    {
        1 => "Light",
        2 => "Thermostat",
        3 => "Sensor",
        4 => "Smart Plug",
        5 => "Door Lock",
        _ => "Unknown"
    };

    public string Icon => Type switch
    {
        1 => "💡",
        2 => "🌡️",
        3 => "📡",
        4 => "🔌",
        5 => "🚪",
        _ => "🏠"
    };
}