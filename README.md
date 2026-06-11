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
5. Open **Settings**, enter an OpenAI API key, and save.
6. Use **Browse Folder** to select `automation-planner-v2`.
7. Click **Reload Planner**, select a scenario, click **Load Scenario**, then **Execute**.

Settings are saved locally under the current user's application data folder and API keys are never hardcoded in source.

## Folder-driven planner packages

A planner package must contain:

```text
SKILL.md
references/
tests/
mock-data/
```

The loader recursively reads `references/**/*.md` and discovers scenarios from `mock-data/**/*.json`. Adding new planner packages, reference files, scenarios, or mock data does not require code changes.

## Mock data storage

Mock data lives inside the selected planner package folder, under its `mock-data/` directory. For example, if the user selects `C:\PlannerPackages\AutomationPlannerV2`, scenarios are discovered from `C:\PlannerPackages\AutomationPlannerV2\mock-data\**\*.json`. The app does not copy planner packages or mock data into AppData.

AppData is used only for local user settings such as the last selected planner package path, model name, timeout, and API key. If a team wants centrally managed mock data, place the planner package in a shared repository or network folder and select that folder in the app.

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
