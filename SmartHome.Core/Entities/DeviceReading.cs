using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartHome.Core.Enums;

namespace SmartHome.Core.Entities
{
    public class DeviceReading
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public ReadingType ReadingType { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }

        public Device Device { get; set; } = null!;
    }
}