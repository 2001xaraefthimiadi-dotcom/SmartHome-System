namespace SmartHome.Mobile.Models;

public class DeviceModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public int Type { get; set; }
    public int Status { get; set; }
    public bool IsOnline { get; set; }
    public double PowerConsumption { get; set; }
}