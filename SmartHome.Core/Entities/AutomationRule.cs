using SmartHome.Core.Enums;

namespace SmartHome.Core.Entities
{
    public class AutomationRule
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ConditionType ConditionType { get; set; }

        public double ConditionValue { get; set; }

        // Η συσκευή από την οποία προέρχεται η μέτρηση

        public int? SourceDeviceId { get; set; }

        // Η συσκευή στην οποία θα εκτελεστεί η ενέργεια
        public int TargetDeviceId { get; set; }

        public ActionType Action { get; set; }

        public bool IsActive { get; set; }

        public TimeSpan? ScheduledTime { get; set; }

        public DateTime? LastExecutedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public Device? SourceDevice { get; set; }

        public Device TargetDevice { get; set; } = null!;
    }
}