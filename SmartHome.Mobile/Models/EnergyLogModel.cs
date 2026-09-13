namespace SmartHome.Mobile.Models;

public class EnergyLogModel
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public double ConsumedWatts { get; set; }
    public DateTime RecordedAt { get; set; }
}