using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartHome.Core.Enums;

namespace SmartHome.Core.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public bool IsOnline { get; set; }
        public double PowerConsumption { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<DeviceReading> Readings { get; set; } = new List<DeviceReading>();
        public ICollection<EnergyLog> EnergyLogs { get; set; } = new List<EnergyLog>();
        public ICollection<AutomationRule> AutomationRules { get; set; } = new List<AutomationRule>();
    }
}