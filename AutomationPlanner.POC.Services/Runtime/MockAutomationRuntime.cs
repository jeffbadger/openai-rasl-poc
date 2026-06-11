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

    public async Task<JToken> InvokeToolAsync(string toolName, JObject? arguments = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = NormalizeToolName(toolName);
        var configuredToolResponse = _scenario["MockRuntime"]?["ToolResponses"]?[toolName]
                                     ?? _scenario["MockRuntime"]?["ToolResponses"]?[normalized];
        if (configuredToolResponse is not null) return configuredToolResponse.DeepClone();

        return normalized switch
        {
            "get_screen_state" => await GetScreenStateAsync(cancellationToken),
            "get_excel_structure" => await GetExcelStructureAsync(cancellationToken),
            "get_callable_signatures" => await GetCallableSignaturesAsync(cancellationToken),
            "ask_user" => await AskUserAsync(arguments?["question"]?.ToString() ?? string.Empty, cancellationToken),
            _ => new JObject
            {
                ["error"] = "No mock response configured for tool.",
                ["toolName"] = toolName
            }
        };
    }

    public async Task<JObject> GetToolResponseSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var askUserResponses = _scenario["MockRuntime"]?["AskUserResponses"] as JObject ?? new JObject();
        var explicitToolResponses = _scenario["MockRuntime"]?["ToolResponses"] as JObject ?? new JObject();
        return new JObject
        {
            ["get_screen_state"] = await GetScreenStateAsync(cancellationToken),
            ["get_excel_structure"] = await GetExcelStructureAsync(cancellationToken),
            ["get_callable_signatures"] = await GetCallableSignaturesAsync(cancellationToken),
            ["ask_user"] = new JObject
            {
                ["configured_responses"] = (JObject)askUserResponses.DeepClone(),
                ["default_response"] = "Mock user approved."
            },
            ["explicit_tool_responses"] = (JObject)explicitToolResponses.DeepClone()
        };
    }

    private static string NormalizeToolName(string toolName) => toolName.Trim().ToLowerInvariant() switch
    {
        "getscreenstate" => "get_screen_state",
        "getexcelstructure" => "get_excel_structure",
        "getcallablesignatures" => "get_callable_signatures",
        "askuser" => "ask_user",
        var value => value
    };
}
