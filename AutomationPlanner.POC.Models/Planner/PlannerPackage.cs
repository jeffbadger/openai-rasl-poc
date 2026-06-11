namespace AutomationPlanner.POC.Models.Planner;

public sealed class PlannerPackage
{
    public string RootPath { get; set; } = string.Empty;
    public string MockDataRootPath { get; set; } = string.Empty;
    public string SkillContent { get; set; } = string.Empty;
    public Dictionary<string, string> ReferenceFiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> TestFiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> MockDataFiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public DateTimeOffset LoadedAt { get; set; } = DateTimeOffset.UtcNow;
}
