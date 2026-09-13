namespace SmartHome.Blazor.Models
{
    public class DeviceModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public int Status { get; set; }
        public bool IsOnline { get; set; }
        public double PowerConsumption { get; set; }
    }
}