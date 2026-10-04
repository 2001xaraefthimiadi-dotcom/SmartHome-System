namespace SmartHome.Mobile.Models;

public class AutomationModel
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int ConditionType { get; set; }

    public double ConditionValue { get; set; }

    public int? SourceDeviceId { get; set; }

    public string? SourceDeviceName { get; set; }

    public int TargetDeviceId { get; set; }

    public string? TargetDeviceName { get; set; }

    public int Action { get; set; }

    public bool IsActive { get; set; }

    public TimeSpan? ScheduledTime { get; set; }

    public DateTime? LastExecutedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}