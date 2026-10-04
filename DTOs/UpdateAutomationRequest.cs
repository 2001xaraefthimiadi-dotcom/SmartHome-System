using SmartHome.Core.Enums;

namespace SmartHome.API.DTOs
{
    public class UpdateAutomationRequest
    {
        public string Name { get; set; } = string.Empty;

        public ConditionType ConditionType { get; set; }

        public double ConditionValue { get; set; }

        public int? SourceDeviceId { get; set; }

        public int TargetDeviceId { get; set; }

        public ActionType Action { get; set; }

        public bool IsActive { get; set; }
        public TimeSpan? ScheduledTime { get; set; }
    }
}