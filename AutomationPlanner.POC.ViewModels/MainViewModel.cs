using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.OpenAI;
using AutomationPlanner.POC.Models.Planner;
using AutomationPlanner.POC.Models.Settings;
using AutomationPlanner.POC.ViewModels.Commands;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IPlannerPackageLoader _packageLoader;
    private readonly IPromptAssembler _promptAssembler;
    private readonly IOpenAiPlannerClient _plannerClient;
    private readonly IPlannerValidator _validator;
    private readonly IMockAutomationRuntime _mockAutomationRuntime;
    private readonly ISettingsStore _settingsStore;
    private readonly IExportService _exportService;
    private PlannerPackage? _plannerPackage;
    private ScenarioItemViewModel? _selectedScenario;
    private string _selectedUseCaseFolder = string.Empty;
    private string _selectedPlannerRoot = string.Empty;
    private string _scenarioJson = string.Empty;
    private string _mockDataRelativePath = string.Empty;
    private string _promptAssembly = string.Empty;
    private string _rawRequest = string.Empty;
    private string _rawResponse = string.Empty;
    private string _plannerJson = string.Empty;
    private string _validationText = string.Empty;
    private string _consoleText = string.Empty;
    private string _diagnosticsText = string.Empty;
    private string _askUserDefaultResponse = "Mock user approved.";
    private AppSettings _settings = new();

    public MainViewModel(IPlannerPackageLoader packageLoader, IPromptAssembler promptAssembler, IOpenAiPlannerClient plannerClient, IPlannerValidator validator, IMockAutomationRuntime mockAutomationRuntime, ISettingsStore settingsStore, IExportService exportService)
    {
        _packageLoader = packageLoader;
        _promptAssembler = promptAssembler;
        _plannerClient = plannerClient;
        _validator = validator;
        _mockAutomationRuntime = mockAutomationRuntime;
        _settingsStore = settingsStore;
        _exportService = exportService;
        ReloadPlannerCommand = new AsyncRelayCommand(ReloadPlannerAsync, () => !string.IsNullOrWhiteSpace(SelectedPlannerRoot));
        LoadScenarioCommand = new RelayCommand(LoadSelectedScenario, () => SelectedScenario is not null);
        NewMockDataCommand = new RelayCommand(BeginNewMockData, () => _plannerPackage is not null);
        SaveMockDataCommand = new AsyncRelayCommand(SaveMockDataAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(MockDataRelativePath) && !string.IsNullOrWhiteSpace(ScenarioJson));
        DeleteMockDataCommand = new AsyncRelayCommand(DeleteMockDataAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(MockDataRelativePath));
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(ScenarioJson));
        RunAllCommand = new AsyncRelayCommand(RunAllAsync, () => _plannerPackage is not null && Scenarios.Count > 0);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !string.IsNullOrWhiteSpace(PromptAssembly));
        SettingsViewModel = new SettingsViewModel(settingsStore);
    }

    public ObservableCollection<string> PackageTree { get; } = [];
    public ObservableCollection<string> UseCaseFolders { get; } = [];
    public ObservableCollection<ScenarioItemViewModel> Scenarios { get; } = [];
    public SettingsViewModel SettingsViewModel { get; }
    public ICommand ReloadPlannerCommand { get; }
    public ICommand LoadScenarioCommand { get; }
    public ICommand NewMockDataCommand { get; }
    public ICommand SaveMockDataCommand { get; }
    public ICommand DeleteMockDataCommand { get; }
    public ICommand ExecuteCommand { get; }
    public ICommand RunAllCommand { get; }
    public ICommand ExportCommand { get; }

    public string SelectedPlannerRoot { get => _selectedPlannerRoot; set { if (SetProperty(ref _selectedPlannerRoot, value)) RaiseCommands(); } }

    public string SelectedUseCaseFolder
    {
        get => _selectedUseCaseFolder;
        set
        {
            if (SetProperty(ref _selectedUseCaseFolder, value))
            {
                DiscoverScenarios();
                AppendConsole($"Selected execution use case folder: {GetUseCaseDisplayName(value)}");
                RaiseCommands();
            }
        }
    }
    public ScenarioItemViewModel? SelectedScenario
    {
        get => _selectedScenario;
        set
        {
            if (SetProperty(ref _selectedScenario, value))
            {
                if (value is not null)
                {
                    MockDataRelativePath = value.RelativePath;
                    ScenarioJson = value.Scenario.Json;
                }
                RaiseCommands();
            }
        }
    }
    public string ScenarioJson { get => _scenarioJson; set { if (SetProperty(ref _scenarioJson, value)) RaiseCommands(); } }
    public string MockDataRelativePath { get => _mockDataRelativePath; set { if (SetProperty(ref _mockDataRelativePath, value)) RaiseCommands(); } }
    public string PromptAssembly { get => _promptAssembly; set => SetProperty(ref _promptAssembly, value); }
    public string RawRequest { get => _rawRequest; set => SetProperty(ref _rawRequest, value); }
    public string RawResponse { get => _rawResponse; set => SetProperty(ref _rawResponse, value); }
    public string PlannerJson { get => _plannerJson; set => SetProperty(ref _plannerJson, value); }
    public string ValidationText { get => _validationText; set => SetProperty(ref _validationText, value); }
    public string ConsoleText { get => _consoleText; set => SetProperty(ref _consoleText, value); }
    public string DiagnosticsText { get => _diagnosticsText; set => SetProperty(ref _diagnosticsText, value); }
    public string AskUserDefaultResponse { get => _askUserDefaultResponse; set => SetProperty(ref _askUserDefaultResponse, value); }
    public AppSettings Settings { get => _settings; set => SetProperty(ref _settings, value); }

    public async Task InitializeAsync()
    {
        Settings = await _settingsStore.LoadAsync();
        SettingsViewModel.Settings = Settings;
        SelectedPlannerRoot = !string.IsNullOrWhiteSpace(Settings.PlannerPackagePath)
            ? Settings.PlannerPackagePath
            : Settings.LastPlannerPackagePath;
    }

    public async Task ReloadPlannerAsync()
    {
        AppendConsole($"Loading planner package: {SelectedPlannerRoot}");
        _plannerPackage = await _packageLoader.LoadAsync(SelectedPlannerRoot, Settings.MockDataBasePath);
        Settings.PlannerPackagePath = SelectedPlannerRoot;
        Settings.LastPlannerPackagePath = SelectedPlannerRoot;
        await _settingsStore.SaveAsync(Settings);
        RefreshPackageTree();
        RefreshUseCaseFolders();
        DiscoverScenarios();
        UpdateDiagnostics();
        AppendConsole($"Loaded {_plannerPackage.ReferenceFiles.Count} references and {Scenarios.Count} scenarios.");
        RaiseCommands();
    }

    public void SetAskUserResponder(Func<string, CancellationToken, Task<string>> responder)
    {
        _mockAutomationRuntime.SetAskUserResponder(responder);
    }

    public void LoadSelectedScenario()
    {
        if (SelectedScenario is null) return;
        ScenarioJson = SelectedScenario.Scenario.Json;
        MockDataRelativePath = SelectedScenario.RelativePath;
        AppendConsole($"Loaded scenario: {SelectedScenario.RelativePath}");
    }

    public async Task ExecuteAsync()
    {
        if (_plannerPackage is null) return;
        var scenario = ParseScenario("Ad hoc scenario", ScenarioJson);
        _mockAutomationRuntime.LoadScenario(scenario.Parsed!);
        _mockAutomationRuntime.SetAskUserDefaultResponse(AskUserDefaultResponse);
        var toolSnapshot = await _mockAutomationRuntime.GetToolResponseSnapshotAsync();
        var executionScenario = CreateScenarioWithToolResponses(scenario, toolSnapshot);
        AppendConsole("Loaded per-tool response packets; ask_user questions will be answered through the app prompt UI.");
        var assembly = _promptAssembler.Assemble(_plannerPackage, executionScenario, "Create an automation plan for the supplied scenario.");
        PromptAssembly = assembly.AssembledPrompt;
        UpdateDiagnostics(assembly);
        AppendConsole($"Prompt assembled. Estimated tokens: {assembly.EstimatedTokens}.");

        OpenAiPlannerResult result = await _plannerClient.CreatePlanAsync(assembly.AssembledPrompt, Settings, _mockAutomationRuntime);
        RawRequest = result.RawRequest;
        RawResponse = result.RawResponse;
        PlannerJson = result.OutputText;
        AppendConsole($"OpenAI call completed in {result.Duration.TotalSeconds:N1}s. Input tokens: {result.InputTokens}; output tokens: {result.OutputTokens}.");
        ValidateCurrentPlannerJson();
    }

    public async Task RunAllAsync()
    {
        if (_plannerPackage is null) return;
        var pass = 0;
        var fail = 0;
        foreach (var item in Scenarios)
        {
            SelectedScenario = item;
            ScenarioJson = item.Scenario.Json;
            try
            {
                await ExecuteAsync();
                if (ValidationText.StartsWith("PASS", StringComparison.OrdinalIgnoreCase)) pass++; else fail++;
            }
            catch (Exception ex)
            {
                fail++;
                AppendConsole($"Scenario failed: {item.Name}: {ex.Message}");
            }
        }
        AppendConsole($"Batch complete. Passed: {pass}. Failed: {fail}.");
    }

    private void BeginNewMockData()
    {
        SelectedScenario = null;
        var selectedFolder = string.IsNullOrWhiteSpace(SelectedUseCaseFolder) ? "mock-data" : SelectedUseCaseFolder.TrimEnd('/');
        MockDataRelativePath = $"{selectedFolder}/new-scenario.json";
        ScenarioJson = string.Join(Environment.NewLine,
            "{",
            "  \"Name\": \"New Scenario\",",
            "  \"Goal\": \"Describe the automation goal.\",",
            "  \"SurfaceType\": \"Web\",",
            "  \"ComponentType\": \"Form\",",
            "  \"Applications\": [],",
            "  \"InitialState\": {},",
            "  \"ExpectedOutcome\": {}",
            "}");
        AppendConsole("Started a new mock-data JSON document.");
    }

    private async Task SaveMockDataAsync()
    {
        if (_plannerPackage is null) return;

        try
        {
            var parsed = JObject.Parse(ScenarioJson);
            var formattedJson = parsed.ToString(Newtonsoft.Json.Formatting.Indented);
            var filePath = GetMockDataFilePath(MockDataRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, formattedJson);

            var savedKey = GetMockDataKey(filePath);
            AppendConsole($"Saved mock-data scenario: {savedKey}");
            await RefreshPlannerPackageAfterMockDataChangeAsync(savedKey, formattedJson);
        }
        catch (Exception ex)
        {
            AppendConsole($"Save failed: {ex.Message}");
        }
    }

    private async Task DeleteMockDataAsync()
    {
        if (_plannerPackage is null) return;

        try
        {
            var filePath = GetMockDataFilePath(MockDataRelativePath);
            if (!File.Exists(filePath))
            {
                AppendConsole($"Mock-data scenario was already missing: {MockDataRelativePath}");
                return;
            }

            var deletedKey = GetMockDataKey(filePath);
            File.Delete(filePath);
            AppendConsole($"Deleted mock-data scenario: {deletedKey}");
            await RefreshPlannerPackageAfterMockDataChangeAsync();
            ScenarioJson = string.Empty;
            MockDataRelativePath = string.Empty;
        }
        catch (Exception ex)
        {
            AppendConsole($"Delete failed: {ex.Message}");
        }
    }

    private async Task RefreshPlannerPackageAfterMockDataChangeAsync(string? selectedKey = null, string? scenarioJson = null)
    {
        _plannerPackage = await _packageLoader.LoadAsync(SelectedPlannerRoot, Settings.MockDataBasePath);
        RefreshPackageTree();
        RefreshUseCaseFolders(GetUseCaseFolderForScenarioKey(selectedKey));
        DiscoverScenarios(selectedKey);
        if (!string.IsNullOrWhiteSpace(selectedKey))
        {
            MockDataRelativePath = selectedKey;
            ScenarioJson = scenarioJson ?? SelectedScenario?.Scenario.Json ?? ScenarioJson;
        }
        UpdateDiagnostics();
        RaiseCommands();
    }

    private async Task ExportAsync()
    {
        var exportRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AutomationPlannerExports", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        await _exportService.SaveTextAsync(Path.Combine(exportRoot, "prompt.md"), PromptAssembly);
        await _exportService.SaveTextAsync(Path.Combine(exportRoot, "raw-request.json"), RawRequest);
        await _exportService.SaveTextAsync(Path.Combine(exportRoot, "raw-response.json"), RawResponse);
        await _exportService.SaveTextAsync(Path.Combine(exportRoot, "planner.json"), PlannerJson);
        await _exportService.SaveTextAsync(Path.Combine(exportRoot, "validation.txt"), ValidationText);
        AppendConsole($"Exported run artifacts to {exportRoot}");
    }

    private void ValidateCurrentPlannerJson()
    {
        var validation = _validator.Validate(PlannerJson);
        PlannerJson = string.IsNullOrWhiteSpace(validation.NormalizedJson) ? PlannerJson : validation.NormalizedJson;
        ValidationText = validation.IsValid
            ? "PASS\n" + string.Join(Environment.NewLine, validation.Warnings)
            : "FAIL\n" + string.Join(Environment.NewLine, validation.Errors);
        AppendConsole($"Validation status: {(validation.IsValid ? "PASS" : "FAIL")}");
        UpdateDiagnostics();
    }

    private void RefreshPackageTree()
    {
        PackageTree.Clear();
        if (_plannerPackage is null) return;
        PackageTree.Add(_plannerPackage.RootPath);
        PackageTree.Add("SKILL.md");
        PackageTree.Add($"references ({_plannerPackage.ReferenceFiles.Count})");
        foreach (var reference in _plannerPackage.ReferenceFiles.Keys) PackageTree.Add("  " + reference);
        PackageTree.Add($"tests ({_plannerPackage.TestFiles.Count})");
        foreach (var test in _plannerPackage.TestFiles.Keys) PackageTree.Add("  " + test);
        PackageTree.Add($"mock-data ({_plannerPackage.MockDataFiles.Count})");
        PackageTree.Add($"  root: {_plannerPackage.MockDataRootPath}");
        foreach (var mock in _plannerPackage.MockDataFiles.Keys) PackageTree.Add("  " + mock);
    }

    private void RefreshUseCaseFolders(string? selectedFolder = null)
    {
        UseCaseFolders.Clear();
        if (_plannerPackage is null) return;

        var folders = new SortedSet<string>(StringComparer.OrdinalIgnoreCase) { "mock-data" };
        if (Directory.Exists(_plannerPackage.MockDataRootPath))
        {
            foreach (var directory in Directory.EnumerateDirectories(_plannerPackage.MockDataRootPath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(_plannerPackage.MockDataRootPath, directory).Replace(Path.DirectorySeparatorChar, '/');
                if (!string.IsNullOrWhiteSpace(relative) && relative != ".")
                {
                    folders.Add($"mock-data/{relative}");
                }
            }
        }

        foreach (var folder in folders)
        {
            UseCaseFolders.Add(folder);
        }

        var nextSelection = !string.IsNullOrWhiteSpace(selectedFolder) && UseCaseFolders.Contains(selectedFolder, StringComparer.OrdinalIgnoreCase)
            ? UseCaseFolders.First(x => string.Equals(x, selectedFolder, StringComparison.OrdinalIgnoreCase))
            : UseCaseFolders.FirstOrDefault() ?? string.Empty;

        if (!string.Equals(_selectedUseCaseFolder, nextSelection, StringComparison.OrdinalIgnoreCase))
        {
            _selectedUseCaseFolder = nextSelection;
            OnPropertyChanged(nameof(SelectedUseCaseFolder));
        }
    }

    private void DiscoverScenarios(string? selectedKey = null)
    {
        Scenarios.Clear();
        if (_plannerPackage is null) return;
        foreach (var file in _plannerPackage.MockDataFiles
                     .Where(x => IsScenarioInSelectedUseCaseFolder(x.Key))
                     .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var scenario = ParseScenario(Path.GetFileNameWithoutExtension(file.Key), file.Value);
                scenario.RelativePath = file.Key;
                Scenarios.Add(new ScenarioItemViewModel(scenario));
            }
            catch (Exception ex)
            {
                AppendConsole($"Skipping invalid scenario {file.Key}: {ex.Message}");
            }
        }
        var nextScenario = string.IsNullOrWhiteSpace(selectedKey)
            ? Scenarios.FirstOrDefault()
            : Scenarios.FirstOrDefault(x => string.Equals(x.RelativePath, selectedKey, StringComparison.OrdinalIgnoreCase)) ?? Scenarios.FirstOrDefault();
        SelectedScenario = nextScenario;
        if (nextScenario is null)
        {
            ScenarioJson = string.Empty;
            MockDataRelativePath = string.Empty;
        }
    }

    private bool IsScenarioInSelectedUseCaseFolder(string scenarioKey)
    {
        if (string.IsNullOrWhiteSpace(SelectedUseCaseFolder) || string.Equals(SelectedUseCaseFolder, "mock-data", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var selectedFolderPrefix = SelectedUseCaseFolder.TrimEnd('/') + "/";
        return scenarioKey.StartsWith(selectedFolderPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetUseCaseFolderForScenarioKey(string? scenarioKey)
    {
        if (string.IsNullOrWhiteSpace(scenarioKey)) return null;

        var normalized = scenarioKey.Replace('\\', '/');
        var lastSlash = normalized.LastIndexOf('/');
        return lastSlash <= "mock-data".Length ? "mock-data" : normalized[..lastSlash];
    }

    private static string GetUseCaseDisplayName(string folder)
    {
        return string.IsNullOrWhiteSpace(folder) || string.Equals(folder, "mock-data", StringComparison.OrdinalIgnoreCase)
            ? "mock-data (all scenarios)"
            : folder;
    }

    private static ScenarioDocument ParseScenario(string name, string json)
    {
        return new ScenarioDocument { Name = name, Json = json, Parsed = JObject.Parse(json) };
    }

    private static ScenarioDocument CreateScenarioWithToolResponses(ScenarioDocument scenario, JObject toolResponses)
    {
        var enriched = (JObject)(scenario.Parsed?.DeepClone() ?? JObject.Parse(scenario.Json));
        enriched["MockToolResponses"] = toolResponses;
        return new ScenarioDocument
        {
            Name = scenario.Name,
            RelativePath = scenario.RelativePath,
            Parsed = enriched,
            Json = enriched.ToString(Newtonsoft.Json.Formatting.Indented)
        };
    }

    private string GetMockDataFilePath(string relativePath)
    {
        if (_plannerPackage is null) throw new InvalidOperationException("Load a planner package before editing mock data.");

        var normalized = NormalizeMockDataRelativePath(relativePath);
        if (!normalized.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".json";
        }

        var root = Path.GetFullPath(_plannerPackage.MockDataRootPath);
        var fullPath = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Mock-data paths must stay inside the configured mock-data folder.");
        }

        return fullPath;
    }

    private string GetMockDataKey(string filePath)
    {
        if (_plannerPackage is null) throw new InvalidOperationException("Load a planner package before editing mock data.");

        var relative = Path.GetRelativePath(_plannerPackage.MockDataRootPath, filePath).Replace(Path.DirectorySeparatorChar, '/');
        return $"mock-data/{relative}";
    }

    private static string NormalizeMockDataRelativePath(string relativePath)
    {
        var normalized = (relativePath ?? string.Empty).Trim().Replace('\\', '/');
        while (normalized.StartsWith("/", StringComparison.Ordinal)) normalized = normalized[1..];
        if (normalized.StartsWith("mock-data/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["mock-data/".Length..];
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Enter a mock-data JSON file name before saving.");
        }

        return normalized;
    }

    private void UpdateDiagnostics(PromptAssembly? assembly = null)
    {
        DiagnosticsText = $"Planner Package Loaded: {_plannerPackage is not null}\n" +
                          $"Reference Count: {_plannerPackage?.ReferenceFiles.Count ?? 0}\n" +
                          $"Scenario Count: {Scenarios.Count}\n" +
                          $"Prompt Size: {assembly?.AssembledPrompt.Length ?? PromptAssembly.Length} chars\n" +
                          $"Estimated Tokens: {assembly?.EstimatedTokens ?? 0}\n" +
                          $"Validation Status: {(ValidationText.StartsWith("PASS", StringComparison.OrdinalIgnoreCase) ? "PASS" : string.IsNullOrWhiteSpace(ValidationText) ? "Not run" : "FAIL")}";
    }

    private void AppendConsole(string message) => ConsoleText += $"[{DateTimeOffset.Now:HH:mm:ss}] {message}{Environment.NewLine}";

    private void RaiseCommands()
    {
        (ReloadPlannerCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (LoadScenarioCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (NewMockDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveMockDataCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DeleteMockDataCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExecuteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (RunAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
