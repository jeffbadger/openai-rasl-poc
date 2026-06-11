namespace AutomationPlanner.POC.Models.Planner;

public sealed class PromptAssembly
{
    public string SystemHeader { get; set; } = "You are an automation planner.";
    public string AssembledPrompt { get; set; } = string.Empty;
    public IReadOnlyList<string> IncludedReferences { get; set; } = [];
    public int EstimatedTokens { get; set; }
}
