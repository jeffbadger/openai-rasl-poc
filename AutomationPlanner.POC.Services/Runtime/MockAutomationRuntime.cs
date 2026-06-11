using AutomationPlanner.POC.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Services.Runtime;

public sealed class MockAutomationRuntime : IMockAutomationRuntime
{
    private JObject _scenario = new();

    public void LoadScenario(JObject scenario) => _scenario = scenario;

    public Task<JObject> GetScreenStateAsync(CancellationToken cancellationToken = default)
    {
        var value = _scenario["MockRuntime"]?["ScreenState"] as JObject
                    ?? _scenario["ApplicationHierarchy"] as JObject
                    ?? new JObject();
        return Task.FromResult((JObject)value.DeepClone());
    }

    public Task<JObject> GetExcelStructureAsync(CancellationToken cancellationToken = default)
    {
        var value = _scenario["MockRuntime"]?["ExcelStructure"] as JObject ?? new JObject();
        return Task.FromResult((JObject)value.DeepClone());
    }

    public Task<JArray> GetCallableSignaturesAsync(CancellationToken cancellationToken = default)
    {
        var value = _scenario["MockRuntime"]?["CallableSignatures"] as JArray ?? new JArray();
        return Task.FromResult((JArray)value.DeepClone());
    }

    public Task<string> AskUserAsync(string question, CancellationToken cancellationToken = default)
    {
        var answer = _scenario["MockRuntime"]?["AskUserResponses"]?[question]?.ToString() ?? "Mock user approved.";
        return Task.FromResult(answer);
    }
}
