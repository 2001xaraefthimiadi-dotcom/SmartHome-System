using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartHome.Core.Enums;

namespace SmartHome.Core.Entities
{
    public class AutomationRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ConditionType ConditionType { get; set; }
        public double ConditionValue { get; set; }
        public int TargetDeviceId { get; set; }
        public ActionType Action { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public Device TargetDevice { get; set; } = null!;
    }
}