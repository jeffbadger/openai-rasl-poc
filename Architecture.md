# Architecture

## Design goals

The POC is designed around provider-neutral interfaces so the desktop shell can grow into a full automation planning platform. Planner packages are dynamic filesystem inputs, not hardcoded application resources.

## Execution flow

1. The user selects a planner package root.
2. `IPlannerPackageLoader` reads `SKILL.md`, recursive markdown references, test files, and mock scenario JSON files from either the package `mock-data/` folder or a configured external mock data base folder.
3. The UI lists package contents and scenarios.
4. The user loads and optionally edits scenario JSON.
5. `IPromptAssembler` composes the final prompt in this order:
   - System header
   - `SKILL.md`
   - selected reference files
   - scenario JSON
   - user request
6. `IOpenAiPlannerClient` sends the prompt to the OpenAI Responses API.
7. `IPlannerValidator` validates that the response is JSON and includes the planner contract fields.
8. Results are displayed in prompt, raw request, raw response, planner JSON, validation, diagnostics, and console views.

## Extensibility seams

- `IReferenceSelectionStrategy` currently loads all references and can later select references by `SurfaceType`, `TaskPrefix`, or planner context.
- `IOpenAiPlannerClient` isolates OpenAI-specific HTTP behavior from the rest of the app. Additional provider interfaces/implementations can be added for Azure OpenAI, local models, Anthropic, Gemini, or MCP-backed tools.
- `IMockAutomationRuntime` simulates screen, Excel, callable, and user-interaction tools from scenario mock data.
- Strongly typed planner response models support inheritance for decision, method, application, loop, label, and todo steps.

## Persistence

`JsonSettingsStore` saves local settings under the user application data folder. Secrets are user-provided at runtime and are not checked into the repository.

Planner packages, references, tests, and mock scenario data are not persisted or duplicated into AppData. The planner package path and optional mock data base path are user-configurable settings; package files and mock scenarios are loaded directly from those folders on reload/execution. AppData stores only preferences, including the selected planner package path and mock data base path.
