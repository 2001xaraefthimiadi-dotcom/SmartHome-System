namespace SmartHome.API.DTOs
{
    public class CreateEnergyLogRequest
    {
        public int DeviceId { get; set; }
        public double ConsumedWatts { get; set; }
    }
}