namespace SmartHome.Blazor.Models
{
    public class AutomationModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ConditionType { get; set; }
        public double ConditionValue { get; set; }
        public int TargetDeviceId { get; set; }
        public int Action { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SourceDeviceId { get; set; }
        public string? SourceDeviceName { get; set; }
        public TimeOnly? ScheduledTime { get; set; }
        public DateTime? LastExecutedAt { get; set; }
    }
}