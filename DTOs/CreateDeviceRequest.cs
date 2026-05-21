using SmartHome.Core.Enums;

namespace SmartHome.API.DTOs
{
    public class CreateDeviceRequest
    {
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public double PowerConsumption { get; set; }
    }
}