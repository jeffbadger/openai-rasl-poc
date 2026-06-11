namespace AutomationPlanner.POC.Models.Planner;

public sealed class PlannerValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public string NormalizedJson { get; set; } = string.Empty;
}
