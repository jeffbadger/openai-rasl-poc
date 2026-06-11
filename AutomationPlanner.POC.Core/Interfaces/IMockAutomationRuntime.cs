using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IMockAutomationRuntime
{
    void LoadScenario(JObject scenario);
    Task<JObject> GetScreenStateAsync(CancellationToken cancellationToken = default);
    Task<JObject> GetExcelStructureAsync(CancellationToken cancellationToken = default);
    Task<JArray> GetCallableSignaturesAsync(CancellationToken cancellationToken = default);
    Task<string> AskUserAsync(string question, CancellationToken cancellationToken = default);
}
