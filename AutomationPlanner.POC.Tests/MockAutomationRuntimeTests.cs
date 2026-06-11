using AutomationPlanner.POC.Services.Runtime;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AutomationPlanner.POC.Tests;

public sealed class MockAutomationRuntimeTests
{
    [Fact]
    public async Task InvokeToolAsync_ReturnsConfiguredMockDataResponses()
    {
        var runtime = new MockAutomationRuntime();
        runtime.LoadScenario(JObject.Parse("""
        {
          "ApplicationHierarchy": { "Fallback": true },
          "MockRuntime": {
            "ScreenState": { "Window": "Login" },
            "ExcelStructure": { "Workbook": "Invoices.xlsx" },
            "CallableSignatures": [{ "Name": "get_account_status" }],
            "ToolResponses": {
              "custom_tool": { "Value": 42 }
            }
          }
        }
        """));

        var screenState = await runtime.InvokeToolAsync("get_screen_state");
        var excelStructure = await runtime.InvokeToolAsync("get_excel_structure");
        var callableSignatures = await runtime.InvokeToolAsync("get_callable_signatures");
        runtime.SetAskUserDefaultResponse("Answered from app.");
        var askUser = await runtime.InvokeToolAsync("ask_user", new JObject { ["question"] = "Continue?" });
        var custom = await runtime.InvokeToolAsync("custom_tool");

        Assert.Equal("Login", screenState["Window"]?.ToString());
        Assert.Equal("Invoices.xlsx", excelStructure["Workbook"]?.ToString());
        Assert.Equal("get_account_status", callableSignatures[0]?["Name"]?.ToString());
        Assert.Equal("Answered from app.", askUser.ToString());
        Assert.Equal(42, custom["Value"]?.Value<int>());
    }

    [Fact]
    public async Task AskUserAsync_UsesConfiguredResponderWhenAvailable()
    {
        var runtime = new MockAutomationRuntime();
        string? capturedQuestion = null;
        runtime.SetAskUserDefaultResponse("Default answer.");
        runtime.SetAskUserResponder((question, _) =>
        {
            capturedQuestion = question;
            return Task.FromResult("UI answer.");
        });

        var answer = await runtime.AskUserAsync("Which queue should be processed?");

        Assert.Equal("Which queue should be processed?", capturedQuestion);
        Assert.Equal("UI answer.", answer);
    }

    [Fact]
    public async Task GetToolResponseSnapshotAsync_ExposesPerToolPacketsForPromptAssembly()
    {
        var runtime = new MockAutomationRuntime();
        runtime.LoadScenario(JObject.Parse("""
        {
          "MockRuntime": {
            "ScreenState": { "VisibleControls": ["Username"] },
            "ToolResponses": {
              "custom_tool": { "Value": 42 }
            }
          }
        }
        """));
        runtime.SetAskUserDefaultResponse("Use app answer.");

        var snapshot = await runtime.GetToolResponseSnapshotAsync();

        Assert.Equal("Username", snapshot["ToolResponseByName"]?["get_screen_state"]?["Response"]?["VisibleControls"]?[0]?.ToString());
        Assert.Equal(42, snapshot["ToolResponseByName"]?["custom_tool"]?["Response"]?["Value"]?.Value<int>());
        Assert.Equal(4, snapshot["ToolPackets"]?.Count());
    }
}
