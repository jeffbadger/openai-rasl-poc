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
            "AskUserResponses": { "Continue?": "Yes" },
            "ToolResponses": {
              "custom_tool": { "Value": 42 }
            }
          }
        }
        """));

        var screenState = await runtime.InvokeToolAsync("get_screen_state");
        var excelStructure = await runtime.InvokeToolAsync("get_excel_structure");
        var callableSignatures = await runtime.InvokeToolAsync("get_callable_signatures");
        var askUser = await runtime.InvokeToolAsync("ask_user", new JObject { ["question"] = "Continue?" });
        var custom = await runtime.InvokeToolAsync("custom_tool");

        Assert.Equal("Login", screenState["Window"]?.ToString());
        Assert.Equal("Invoices.xlsx", excelStructure["Workbook"]?.ToString());
        Assert.Equal("get_account_status", callableSignatures[0]?["Name"]?.ToString());
        Assert.Equal("Yes", askUser.ToString());
        Assert.Equal(42, custom["Value"]?.Value<int>());
    }

    [Fact]
    public async Task GetToolResponseSnapshotAsync_ExposesMockDataForPromptAssembly()
    {
        var runtime = new MockAutomationRuntime();
        runtime.LoadScenario(JObject.Parse("""
        {
          "MockRuntime": {
            "ScreenState": { "VisibleControls": ["Username"] },
            "AskUserResponses": { "Credentials available?": "Use test credential." }
          }
        }
        """));

        var snapshot = await runtime.GetToolResponseSnapshotAsync();

        Assert.Equal("Username", snapshot["get_screen_state"]?["VisibleControls"]?[0]?.ToString());
        Assert.Equal("Use test credential.", snapshot["ask_user"]?["configured_responses"]?["Credentials available?"]?.ToString());
    }
}
