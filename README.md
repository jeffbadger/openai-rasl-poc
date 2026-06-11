# Automation Planner POC

A .NET 8 WPF proof-of-concept that loads an Automation Planner skill package from disk and uses it as a dynamic prompt system for the OpenAI Responses API with the default `gpt-5.2` model.

## Projects

- `AutomationPlanner.POC` - WPF shell, main window, settings dialog, composition root.
- `AutomationPlanner.POC.Core` - provider-neutral interfaces.
- `AutomationPlanner.POC.Models` - planner package, planner contract, OpenAI request/response, settings, and validation models.
- `AutomationPlanner.POC.Services` - package loading, prompt assembly, validation, settings, export, and mock runtime services.
- `AutomationPlanner.POC.Infrastructure` - OpenAI Responses API provider implementation.
- `AutomationPlanner.POC.ViewModels` - MVVM view models and commands.
- `AutomationPlanner.POC.Views` - reserved project for reusable WPF controls/views.
- `AutomationPlanner.POC.Tests` - xUnit tests for core services.

## Setup

1. Install the .NET 8 SDK and a Windows environment capable of building WPF.
2. Open `AutomationPlanner.POC.sln` in Visual Studio 2022 or newer.
3. Set `AutomationPlanner.POC` as the startup project.
4. Run the application.
5. Open **Settings**, enter an OpenAI API key, set the planner package folder, optionally set a separate mock data base folder, and save.
6. Alternatively, use **Browse Folder** on the main window to select `automation-planner-v2`.
7. Click **Reload Planner**, choose an **Execution use case folder** (the mock-data root or one of its child folders), select a scenario, click **Load Scenario**, then **Execute**.

Settings are saved locally under the current user's application data folder and API keys are never hardcoded in source.


## Screenshots

The documentation includes illustrative screenshots of the WPF shell to show the primary workflows before running the Windows desktop app.

### Main planner workspace

![Automation Planner POC main window showing the planner package explorer, mock data JSON editor, execution result tabs, and execution console.](docs/screenshots/main-window.svg)

### Settings dialog

![Settings dialog showing OpenAI request options, planner package folder, and optional mock data base folder.](docs/screenshots/settings-window.svg)

## Folder-driven planner packages

A planner package must contain:

```text
SKILL.md
references/
tests/
mock-data/
```

The loader recursively reads `references/**/*.md` and discovers scenarios from either the package `mock-data/**/*.json` folder or the configured external mock data base folder. The main workspace exposes an **Execution use case folder** dropdown populated with the mock-data root and every child folder so execution can be scoped to a specific use-case folder before choosing a scenario. Adding new planner packages, reference files, scenarios, use-case folders, or mock data does not require code changes.

## Planner package and mock data storage

The Settings screen has separate folder fields for the automation planner package and the mock data base folder. The planner package folder points to the directory containing `SKILL.md`, `references/`, and `tests/`. The mock data base folder points to the directory whose recursive JSON files should be treated as scenarios.

If Mock Data Base Folder is blank, mock data defaults to the selected planner package's `mock-data/` directory. For example, if the planner package is `C:\PlannerPackages\AutomationPlannerV2`, scenarios are discovered from `C:\PlannerPackages\AutomationPlannerV2\mock-data\**\*.json`. If Mock Data Base Folder is set to `D:\AutomationMocks`, scenarios are discovered from `D:\AutomationMocks\**\*.json` instead.

The **Execution use case folder** dropdown is built from that resolved mock-data base folder. Selecting `mock-data` shows all scenarios; selecting a child folder such as `mock-data/claims` filters the scenario list and **Run All** to JSON files in that folder subtree. The **Execute** button sends the currently loaded scenario JSON, so edit or load the desired scenario after selecting the folder.

The app does not copy planner packages or mock data into AppData. AppData is used only for local user settings such as the selected planner package folder, mock data base folder, model name, timeout, and API key. If a team wants centrally managed mock data, place the mock data base folder in a shared repository or network folder and select that folder in the app.


## Mock tool responses

Mock automation tools are configured from each scenario's `MockRuntime` object. During execution, the app loads the selected scenario into `MockAutomationRuntime`, resolves the fake tool responses, and injects a `MockToolResponses` snapshot into the prompt scenario JSON so the planner can reason over the same data that the simulated tools would return. The snapshot is packetized: each mock-data response is represented as its own item in `MockToolResponses.ToolPackets`, with a `ToolName`, `Arguments`, `Response`, and `Source`, plus a `ToolResponseByName` convenience map for lookup by tool name.

Supported built-in mock tool keys are:

- `ScreenState` for `get_screen_state()`
- `ExcelStructure` for `get_excel_structure()`
- `CallableSignatures` for `get_callable_signatures()`
- `ToolResponses` for additional named fake tool responses; each named response becomes its own packet

`ask_user(question)` is intentionally not included in `MockToolResponses` and is not backed by scenario mock-data questions because the planner can ask clarifying questions that are hard to anticipate. Instead, the OpenAI request registers `ask_user` as a real Responses API function tool. When the model calls that tool, the WPF app displays an `ask_user` dialog containing the model's question, pre-fills the answer box from the **ask_user app answer** field, and returns the submitted answer as a `function_call_output`.

## How execution triggers a request

The **Execute** button is bound to `ExecuteCommand` in the main view model. WPF invokes that command when the button is clicked, and the command runs `ExecuteAsync` only when a planner package is loaded and the scenario JSON is not blank. `ExecuteAsync` parses the editor JSON, loads it into the mock automation runtime, applies the app-provided `ask_user` default answer, captures packetized mock-data tool responses, assembles the prompt, and then calls `IOpenAiPlannerClient.CreatePlanAsync`. The OpenAI client serializes the request model with the real `ask_user` function tool and posts it to `/v1/responses`; when tool calls are returned, it appends `function_call_output` items and continues the Responses API turn loop until a final planner response is returned. The resulting raw request, raw response, planner JSON, validation output, and console messages are displayed in the execution result tabs.

## OpenAI integration

The OpenAI client posts strongly typed request models to `/v1/responses`, supports cancellation, retries transient failures, records request duration, token usage, raw request JSON, and raw response JSON.

## Exporting

The **Export** button writes prompt, raw request, raw response, planner JSON, and validation output to `Documents/AutomationPlannerExports/<timestamp>/`.

## Testing

On Windows with .NET 8 installed:

```powershell
dotnet restore AutomationPlanner.POC.sln
dotnet build AutomationPlanner.POC.sln
dotnet test AutomationPlanner.POC.Tests/AutomationPlanner.POC.Tests.csproj
```
