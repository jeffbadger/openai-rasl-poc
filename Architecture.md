# Architecture

## Design goals

The POC is designed around provider-neutral interfaces so the desktop shell can grow into a full automation planning platform. Planner packages are dynamic filesystem inputs, not hardcoded application resources.

## Execution flow

1. The user selects a planner package root.
2. `IPlannerPackageLoader` reads `SKILL.md`, recursive markdown references, test files, and mock scenario JSON files from either the package `mock-data/` folder or a configured external mock data base folder.
3. The UI lists package contents, the resolved mock-data root and child folders, and scenarios filtered by the selected execution use-case folder.
4. The user selects a use-case folder, loads a scenario from that scope, and optionally edits scenario JSON.
5. Clicking **Execute** triggers the WPF button binding for `ExecuteCommand`; `AsyncRelayCommand` invokes `MainViewModel.ExecuteAsync` when a planner package is loaded and scenario JSON is available.
6. `IMockAutomationRuntime` loads the scenario mock data and produces packetized `MockToolResponses` for fake tools such as `get_screen_state()`, `get_excel_structure()`, `get_callable_signatures()`, and additional named tool responses; `ask_user()` is excluded from mock-data packets and is handled as a real Responses API function tool.
7. `IPromptAssembler` composes the final prompt in this order:
   - System header
   - `SKILL.md`
   - selected reference files
   - scenario JSON enriched with `MockToolResponses`
   - user request
8. `IOpenAiPlannerClient` serializes the Responses API request model, registers the real `ask_user` function tool, sends requests to `/v1/responses`, and loops over any returned function calls by executing the tool and submitting `function_call_output` items.
9. `IPlannerValidator` validates that the response is JSON and includes the planner contract fields.
10. Results are displayed in prompt, raw request, raw response, planner JSON, validation, diagnostics, and console views.

## Extensibility seams

- `IReferenceSelectionStrategy` currently loads all references and can later select references by `SurfaceType`, `TaskPrefix`, or planner context.
- `IOpenAiPlannerClient` isolates OpenAI-specific HTTP behavior from the rest of the app. Additional provider interfaces/implementations can be added for Azure OpenAI, local models, Anthropic, Gemini, or MCP-backed tools.
- `IMockAutomationRuntime` simulates screen, Excel, and callable tools from scenario `MockRuntime` data, supports additional named fake tool responses through `MockRuntime.ToolResponses`, emits each mock-data response as an individual packet, and delegates real `ask_user()` function calls to an app-provided responder that displays the WPF clarification dialog.
- Strongly typed planner response models support inheritance for decision, method, application, loop, label, and todo steps.

## Persistence

`JsonSettingsStore` saves local settings under the user application data folder. Secrets are user-provided at runtime and are not checked into the repository.

Planner packages, references, tests, and mock scenario data are not persisted or duplicated into AppData. The planner package path and optional mock data base path are user-configurable settings; package files and mock scenarios are loaded directly from those folders on reload/execution. AppData stores only preferences, including the selected planner package path and mock data base path.
