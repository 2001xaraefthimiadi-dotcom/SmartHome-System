using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Core.Entities
{
    public class EnergyLog
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public double ConsumedWatts { get; set; }
        public DateTime RecordedAt { get; set; }

        public Device Device { get; set; } = null!;
    }
}