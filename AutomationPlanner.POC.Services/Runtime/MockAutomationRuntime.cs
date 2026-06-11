using AutomationPlanner.POC.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Services.Runtime;

public sealed class MockAutomationRuntime : IMockAutomationRuntime
{
    private const string DefaultAskUserResponse = "Mock user approved.";
    private JObject _scenario = new();
    private string _askUserDefaultResponse = DefaultAskUserResponse;
    private Func<string, CancellationToken, Task<string>>? _askUserResponder;

    public void LoadScenario(JObject scenario) => _scenario = scenario;

    public void SetAskUserDefaultResponse(string response)
    {
        _askUserDefaultResponse = string.IsNullOrWhiteSpace(response) ? DefaultAskUserResponse : response.Trim();
    }

    public void SetAskUserResponder(Func<string, CancellationToken, Task<string>> responder)
    {
        _askUserResponder = responder;
    }

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

    public async Task<string> AskUserAsync(string question, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_askUserResponder is null) return _askUserDefaultResponse;

        var response = await _askUserResponder(question, cancellationToken);
        return string.IsNullOrWhiteSpace(response) ? _askUserDefaultResponse : response.Trim();
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
        var packets = new JArray
        {
            CreatePacket("get_screen_state", new JObject(), await GetScreenStateAsync(cancellationToken), "MockRuntime.ScreenState or ApplicationHierarchy"),
            CreatePacket("get_excel_structure", new JObject(), await GetExcelStructureAsync(cancellationToken), "MockRuntime.ExcelStructure"),
            CreatePacket("get_callable_signatures", new JObject(), await GetCallableSignaturesAsync(cancellationToken), "MockRuntime.CallableSignatures")
        };

        var byName = new JObject();
        foreach (var packet in packets.OfType<JObject>())
        {
            byName[packet["ToolName"]?.ToString() ?? string.Empty] = packet.DeepClone();
        }

        if (byName["get_screen_state"] is JToken screenStatePacket)
        {
            byName["get_screen_hierarchy"] = screenStatePacket.DeepClone();
            byName["get-screen-hierarchy"] = screenStatePacket.DeepClone();
        }

        var explicitToolResponses = _scenario["MockRuntime"]?["ToolResponses"] as JObject ?? new JObject();
        foreach (var response in explicitToolResponses.Properties())
        {
            var packet = CreatePacket(response.Name, new JObject(), response.Value, "MockRuntime.ToolResponses");
            packets.Add(packet);
            byName[response.Name] = packet.DeepClone();
        }

        return new JObject
        {
            ["ToolPackets"] = packets,
            ["ToolResponseByName"] = byName
        };
    }

    private static JObject CreatePacket(string toolName, JObject arguments, JToken response, string source)
    {
        return new JObject
        {
            ["ToolName"] = toolName,
            ["Arguments"] = (JObject)arguments.DeepClone(),
            ["Response"] = response.DeepClone(),
            ["Source"] = source
        };
    }

    private static string NormalizeToolName(string toolName) => toolName.Trim().ToLowerInvariant() switch
    {
        "getscreenstate" => "get_screen_state",
        "get_screen_hierarchy" => "get_screen_state",
        "getscreenhierarchy" => "get_screen_state",
        "get-screen-hierarchy" => "get_screen_state",
        "getexcelstructure" => "get_excel_structure",
        "getcallablesignatures" => "get_callable_signatures",
        "askuser" => "ask_user",
        var value => value
    };
}
