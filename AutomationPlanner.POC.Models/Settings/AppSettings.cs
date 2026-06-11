namespace AutomationPlanner.POC.Models.Settings;

public sealed class AppSettings
{
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-5.2";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 8192;
    public int RequestTimeoutSeconds { get; set; } = 120;
    public string LastPlannerPackagePath { get; set; } = string.Empty;
}
