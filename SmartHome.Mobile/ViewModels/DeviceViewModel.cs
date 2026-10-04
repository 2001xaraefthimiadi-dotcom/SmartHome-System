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
        Status == 1 ? "● ΕΝΕΡΓΗ" : "● ΑΝΕΝΕΡΓΗ";

    public string OnlineText =>
        IsOnline ? "Συνδεδεμένη" : "Εκτός σύνδεσης";

    public string PowerText =>
        $"{PowerConsumption:N1} W";

    public string TypeText => Type switch
    {
        1 => "Φωτισμός",
        2 => "Θερμοστάτης",
        3 => "Αισθητήρας",
        4 => "Έξυπνη Πρίζα",
        5 => "Έξυπνη Κλειδαριά",
        _ => "Άγνωστος"
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