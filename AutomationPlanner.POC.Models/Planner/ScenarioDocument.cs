using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Models.Planner;

public sealed class ScenarioDocument
{
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string Json { get; set; } = string.Empty;
    public JObject? Parsed { get; set; }
    public string Goal => Parsed?["Goal"]?.ToString() ?? string.Empty;
    public string SurfaceType => Parsed?["SurfaceType"]?.ToString() ?? string.Empty;
    public string ComponentType => Parsed?["ComponentType"]?.ToString() ?? string.Empty;
}
