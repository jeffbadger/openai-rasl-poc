using AutomationPlanner.POC.Models.OpenAI;
using AutomationPlanner.POC.Models.Settings;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IOpenAiPlannerClient
{
    Task<OpenAiPlannerResult> CreatePlanAsync(string prompt, AppSettings settings, CancellationToken cancellationToken = default);
}
