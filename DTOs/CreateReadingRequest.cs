using SmartHome.Core.Enums;

namespace SmartHome.API.DTOs
{
    public class CreateReadingRequest
    {
        public int DeviceId { get; set; }
        public ReadingType ReadingType { get; set; }
        public double Value { get; set; }
    }
}