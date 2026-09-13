namespace SmartHome.Mobile.Models;

public class AutomationModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ConditionType { get; set; }
    public double ConditionValue { get; set; }
    public int TargetDeviceId { get; set; }
    public int Action { get; set; }
    public bool IsActive { get; set; }
}