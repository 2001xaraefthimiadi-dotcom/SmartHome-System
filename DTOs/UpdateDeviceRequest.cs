using SmartHome.Core.Enums;

namespace SmartHome.API.DTOs
{
    public class UpdateDeviceRequest
    {
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public bool IsOnline { get; set; }
        public double PowerConsumption { get; set; }
    }
}