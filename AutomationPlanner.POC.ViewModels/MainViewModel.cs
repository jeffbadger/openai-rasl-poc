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
    private const string ScenarioFileType = "Scenario";
    private const string UserRequestFileType = "User Request";
    private const string ApplicationHierarchyFileType = "Application Hierarchy";
    private const string CompletedStepsFileType = "Completed Steps";
    private const string DurableMemoryFileType = "Durable Memory";

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
    private string _selectedMockDataFileType = ScenarioFileType;
    private string _specialFileName = "new-scenario";
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
        LoadScenarioCommand = new RelayCommand(LoadSelectedScenario, () => _plannerPackage is not null && (!string.IsNullOrWhiteSpace(MockDataRelativePath) || SelectedScenario is not null));
        NewMockDataCommand = new RelayCommand(BeginNewMockData, () => _plannerPackage is not null);
        ScaffoldMockDataCommand = new AsyncRelayCommand(ScaffoldMockDataAsync, () => _plannerPackage is not null);
        SaveMockDataCommand = new AsyncRelayCommand(SaveMockDataAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(MockDataRelativePath) && !string.IsNullOrWhiteSpace(ScenarioJson));
        DeleteMockDataCommand = new AsyncRelayCommand(DeleteMockDataAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(MockDataRelativePath));
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(ScenarioJson) && IsScenarioFileType(SelectedMockDataFileType));
        RunAllCommand = new AsyncRelayCommand(RunAllAsync, () => _plannerPackage is not null && Scenarios.Count > 0);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !string.IsNullOrWhiteSpace(PromptAssembly));
        SettingsViewModel = new SettingsViewModel(settingsStore);
    }

    public ObservableCollection<string> PackageTree { get; } = [];
    public ObservableCollection<string> UseCaseFolders { get; } = [];
    public ObservableCollection<ScenarioItemViewModel> Scenarios { get; } = [];
    public ObservableCollection<string> MockDataFileTypes { get; } =
    [
        ScenarioFileType,
        UserRequestFileType,
        ApplicationHierarchyFileType,
        CompletedStepsFileType,
        DurableMemoryFileType
    ];
    public SettingsViewModel SettingsViewModel { get; }
    public ICommand ReloadPlannerCommand { get; }
    public ICommand LoadScenarioCommand { get; }
    public ICommand NewMockDataCommand { get; }
    public ICommand ScaffoldMockDataCommand { get; }
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
                    SpecialFileName = ExtractSpecialFileNameFromRelativePath(value.RelativePath);
                    SelectedMockDataFileType = ScenarioFileType;
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
    public string SelectedMockDataFileType
    {
        get => _selectedMockDataFileType;
        set
        {
            if (SetProperty(ref _selectedMockDataFileType, value))
            {
                SyncMockDataRelativePathFromSelection();
                RaiseCommands();
            }
        }
    }

    public string SpecialFileName
    {
        get => _specialFileName;
        set
        {
            if (SetProperty(ref _specialFileName, value))
            {
                SyncMockDataRelativePathFromSelection();
                RaiseCommands();
            }
        }
    }
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
        if (_plannerPackage is null) return;

        if (!string.IsNullOrWhiteSpace(MockDataRelativePath))
        {
            try
            {
                var filePath = GetMockDataFilePath(MockDataRelativePath);
                if (!File.Exists(filePath))
                {
                    AppendConsole($"File not found: {MockDataRelativePath}");
                    return;
                }

                ScenarioJson = File.ReadAllText(filePath);
                MockDataRelativePath = GetMockDataKey(filePath);
                SpecialFileName = ExtractSpecialFileNameFromRelativePath(MockDataRelativePath);
                SelectedMockDataFileType = GetMockDataFileTypeFromRelativePath(MockDataRelativePath);
                MockDataRelativePath = GetMockDataKey(filePath);
                AppendConsole($"Loaded file: {MockDataRelativePath}");
                return;
            }
            catch (Exception ex)
            {
                AppendConsole($"Load failed: {ex.Message}");
                return;
            }
        }

        if (SelectedScenario is null) return;
        ScenarioJson = SelectedScenario.Scenario.Json;
        MockDataRelativePath = SelectedScenario.RelativePath;
        SpecialFileName = ExtractSpecialFileNameFromRelativePath(SelectedScenario.RelativePath);
        SelectedMockDataFileType = ScenarioFileType;
        MockDataRelativePath = SelectedScenario.RelativePath;
        AppendConsole($"Loaded scenario: {SelectedScenario.RelativePath}");
    }

    public async Task ExecuteAsync()
    {
        if (_plannerPackage is null) return;
        var scenario = ParseScenario("Ad hoc scenario", ScenarioJson, MockDataRelativePath);
        var preparedScenario = await PrepareScenarioForExecutionAsync(scenario);
        var userRequest = await ResolveUserRequestAsync(preparedScenario);
        _mockAutomationRuntime.LoadScenario(preparedScenario.Parsed!);
        _mockAutomationRuntime.SetAskUserDefaultResponse(AskUserDefaultResponse);
        var toolSnapshot = await _mockAutomationRuntime.GetToolResponseSnapshotAsync();
        var executionScenario = CreateScenarioWithToolResponses(preparedScenario, toolSnapshot);
        AppendConsole("Loaded per-tool response packets; ask_user questions will be answered through the app prompt UI.");
        var assembly = _promptAssembler.Assemble(_plannerPackage, executionScenario, userRequest);
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
        if (_plannerPackage is null) return;

        SelectedScenario = null;
        var selectedFolder = string.IsNullOrWhiteSpace(SelectedUseCaseFolder) ? "mock-data" : SelectedUseCaseFolder.TrimEnd('/');
        var specialName = NormalizeSpecialFileName(SpecialFileName);
        SpecialFileName = specialName;
        MockDataRelativePath = BuildSpecialFileRelativePath(selectedFolder, SelectedMockDataFileType, specialName);
        ScenarioJson = BuildNewFileTemplate(SelectedMockDataFileType, specialName);
        AppendConsole($"Started new {SelectedMockDataFileType} file: {MockDataRelativePath}");
    }

    private async Task ScaffoldMockDataAsync()
    {
        if (_plannerPackage is null) return;

        var selectedFolder = string.IsNullOrWhiteSpace(SelectedUseCaseFolder) ? "mock-data" : SelectedUseCaseFolder.TrimEnd('/');
        var specialName = NormalizeSpecialFileName(SpecialFileName);
        SpecialFileName = specialName;

        var pathsAndContent = new (string RelativePath, string Content)[]
        {
            (BuildSpecialFileRelativePath(selectedFolder, ScenarioFileType, specialName), BuildNewFileTemplate(ScenarioFileType, specialName)),
            (BuildSpecialFileRelativePath(selectedFolder, UserRequestFileType, specialName), BuildNewFileTemplate(UserRequestFileType, specialName)),
            (BuildSpecialFileRelativePath(selectedFolder, ApplicationHierarchyFileType, specialName), BuildNewFileTemplate(ApplicationHierarchyFileType, specialName)),
            (BuildSpecialFileRelativePath(selectedFolder, CompletedStepsFileType, specialName), BuildNewFileTemplate(CompletedStepsFileType, specialName)),
            (BuildSpecialFileRelativePath(selectedFolder, DurableMemoryFileType, specialName), BuildNewFileTemplate(DurableMemoryFileType, specialName))
        };

        foreach (var (relativePath, content) in pathsAndContent)
        {
            var filePath = GetMockDataFilePath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, NormalizeContentForSave(GetMockDataFileTypeFromRelativePath(relativePath), content));
        }

        MockDataRelativePath = pathsAndContent[0].RelativePath;
        SelectedMockDataFileType = ScenarioFileType;
        ScenarioJson = BuildNewFileTemplate(ScenarioFileType, specialName);

        await RefreshPlannerPackageAfterMockDataChangeAsync(MockDataRelativePath, ScenarioJson);
        AppendConsole($"Scaffolded scenario bundle: {specialName} in {selectedFolder}");
    }

    private async Task SaveMockDataAsync()
    {
        if (_plannerPackage is null) return;

        try
        {
            var contentToSave = NormalizeContentForSave(SelectedMockDataFileType, ScenarioJson);
            var filePath = GetMockDataFilePath(MockDataRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, contentToSave);

            var savedKey = GetMockDataKey(filePath);
            AppendConsole($"Saved {SelectedMockDataFileType} file: {savedKey}");

            var isScenarioFile = IsScenarioFileType(SelectedMockDataFileType);
            await RefreshPlannerPackageAfterMockDataChangeAsync(isScenarioFile ? savedKey : null, isScenarioFile ? contentToSave : null, preserveEditorSelection: !isScenarioFile);
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
            var isScenarioFile = IsScenarioFileType(SelectedMockDataFileType);
            AppendConsole($"Deleted {SelectedMockDataFileType} file: {deletedKey}");
            await RefreshPlannerPackageAfterMockDataChangeAsync(preserveEditorSelection: !isScenarioFile);
            ScenarioJson = string.Empty;
            MockDataRelativePath = string.Empty;
        }
        catch (Exception ex)
        {
            AppendConsole($"Delete failed: {ex.Message}");
        }
    }

    private async Task RefreshPlannerPackageAfterMockDataChangeAsync(string? selectedKey = null, string? scenarioJson = null, bool preserveEditorSelection = false)
    {
        var currentPath = MockDataRelativePath;
        var currentContent = ScenarioJson;
        _plannerPackage = await _packageLoader.LoadAsync(SelectedPlannerRoot, Settings.MockDataBasePath);
        RefreshPackageTree();
        RefreshUseCaseFolders(GetUseCaseFolderForScenarioKey(selectedKey));
        DiscoverScenarios(selectedKey);
        if (!string.IsNullOrWhiteSpace(selectedKey))
        {
            MockDataRelativePath = selectedKey;
            ScenarioJson = scenarioJson ?? SelectedScenario?.Scenario.Json ?? ScenarioJson;
        }
        else if (preserveEditorSelection)
        {
            MockDataRelativePath = currentPath;
            ScenarioJson = currentContent;
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
                if (!TryParseScenario(Path.GetFileNameWithoutExtension(file.Key), file.Value, file.Key, out var scenario) || scenario is null)
                {
                    continue;
                }

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

    private static bool TryParseScenario(string name, string json, string relativePath, out ScenarioDocument? scenario)
    {
        scenario = null;
        JObject parsed;

        try
        {
            parsed = JObject.Parse(json);
        }
        catch
        {
            return false;
        }

        if (!LooksLikeScenario(parsed))
        {
            return false;
        }

        scenario = new ScenarioDocument
        {
            Name = name,
            RelativePath = relativePath,
            Parsed = parsed,
            Json = parsed.ToString(Newtonsoft.Json.Formatting.Indented)
        };
        return true;
    }

    private static bool LooksLikeScenario(JObject parsed)
    {
        return parsed["Goal"] is not null
               || parsed["UserRequest"] is not null
               || parsed["UserRequestFile"] is not null
               || parsed["SurfaceType"] is not null
               || parsed["ApplicationHierarchyFile"] is not null;
    }

    private async Task<ScenarioDocument> PrepareScenarioForExecutionAsync(ScenarioDocument scenario)
    {
        if (_plannerPackage is null) return scenario;

        var enriched = (JObject)(scenario.Parsed?.DeepClone() ?? JObject.Parse(scenario.Json));
        var applicationHierarchyFile = enriched["ApplicationHierarchyFile"]?.ToString();
        if (!string.IsNullOrWhiteSpace(applicationHierarchyFile))
        {
            var hierarchyPath = ResolveLinkedMockDataFilePath(scenario, applicationHierarchyFile, "ApplicationHierarchyFile");
            var hierarchyJson = await File.ReadAllTextAsync(hierarchyPath);
            enriched["ApplicationHierarchy"] = JToken.Parse(hierarchyJson);
            AppendConsole($"Loaded ApplicationHierarchy from linked file: {applicationHierarchyFile}");
        }

        var completedStepsFile = enriched["CompletedStepsFile"]?.ToString();
        if (!string.IsNullOrWhiteSpace(completedStepsFile))
        {
            var completedStepsPath = ResolveLinkedMockDataFilePath(scenario, completedStepsFile, "CompletedStepsFile");
            var completedStepsText = await File.ReadAllTextAsync(completedStepsPath);
            enriched["CompletedSteps"] = ParseCompletedStepsToken(completedStepsText);
            AppendConsole($"Loaded CompletedSteps from linked file: {completedStepsFile}");
        }

        var durableMemoryFile = enriched["DurableMemoryFile"]?.ToString();
        if (!string.IsNullOrWhiteSpace(durableMemoryFile))
        {
            var durableMemoryPath = ResolveLinkedMockDataFilePath(scenario, durableMemoryFile, "DurableMemoryFile");
            var durableMemoryJson = await File.ReadAllTextAsync(durableMemoryPath);
            var durableMemoryToken = JToken.Parse(durableMemoryJson);
            if (durableMemoryToken is not JObject durableMemoryObject)
            {
                throw new InvalidDataException("DurableMemoryFile must contain a JSON object.");
            }

            enriched["DurableMemory"] = durableMemoryObject;
            AppendConsole($"Loaded DurableMemory from linked file: {durableMemoryFile}");
        }

        return new ScenarioDocument
        {
            Name = scenario.Name,
            RelativePath = scenario.RelativePath,
            Parsed = enriched,
            Json = enriched.ToString(Newtonsoft.Json.Formatting.Indented)
        };
    }

    private static JArray ParseCompletedStepsToken(string text)
    {
        var rawText = text ?? string.Empty;
        var trimmed = rawText.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return [];
        }

        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            var parsedToken = JToken.Parse(trimmed);
            if (parsedToken is JArray parsedArray)
            {
                return parsedArray;
            }

            throw new InvalidDataException("CompletedStepsFile JSON must be an array of step summaries.");
        }

        var lines = rawText
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.StartsWith("- ", StringComparison.Ordinal) || x.StartsWith("* ", StringComparison.Ordinal) ? x[2..].Trim() : x);

        return new JArray(lines);
    }

    private async Task<string> ResolveUserRequestAsync(ScenarioDocument scenario)
    {
        const string defaultRequest = "Create an automation plan for the supplied scenario.";
        var parsed = scenario.Parsed ?? JObject.Parse(scenario.Json);

        var userRequestFile = parsed["UserRequestFile"]?.ToString();
        if (!string.IsNullOrWhiteSpace(userRequestFile))
        {
            var requestPath = ResolveLinkedMockDataFilePath(scenario, userRequestFile, "UserRequestFile");
            var fileRequest = (await File.ReadAllTextAsync(requestPath)).Trim();
            if (!string.IsNullOrWhiteSpace(fileRequest))
            {
                AppendConsole($"Loaded user request from linked file: {userRequestFile}");
                return fileRequest;
            }
        }

        var inlineRequest = parsed["UserRequest"]?.ToString()?.Trim();
        if (!string.IsNullOrWhiteSpace(inlineRequest))
        {
            return inlineRequest;
        }

        var structuredRequest = BuildStructuredScenarioRequest(parsed);
        if (!string.IsNullOrWhiteSpace(structuredRequest))
        {
            AppendConsole("Built user request from scenario fields (Goal/SurfaceType/TaskPrefix/CompletedSteps/DurableMemory).");
            return structuredRequest;
        }

        return defaultRequest;
    }

    private static string BuildStructuredScenarioRequest(JObject scenario)
    {
        var goal = GetScenarioTextValue(scenario, "Goal");
        var surfaceType = GetScenarioTextValue(scenario, "SurfaceType", "Surface Type");
        var taskPrefix = GetScenarioTextValue(scenario, "TaskPrefix", "Task Prefix", "Task_Prefix");

        var completedStepsToken = GetScenarioToken(scenario, "CompletedSteps", "CompletedStepSummaries");
        var durableMemoryToken = GetScenarioToken(scenario, "DurableMemory");

        if (string.IsNullOrWhiteSpace(goal)
            && string.IsNullOrWhiteSpace(surfaceType)
            && string.IsNullOrWhiteSpace(taskPrefix)
            && IsNullOrEmptyToken(completedStepsToken)
            && IsNullOrEmptyToken(durableMemoryToken))
        {
            return string.Empty;
        }

        var request = new System.Text.StringBuilder();
        AppendRequestSection(request, "Goal", string.IsNullOrWhiteSpace(goal) ? "(not provided)" : goal);
        AppendRequestSection(request, "Surface Type", string.IsNullOrWhiteSpace(surfaceType) ? "(not provided)" : surfaceType);
        AppendRequestSection(request, "Task Prefix", string.IsNullOrWhiteSpace(taskPrefix) ? "(none)" : taskPrefix);
        AppendRequestSection(request, "Completed Steps", FormatRequestToken(completedStepsToken));
        AppendRequestSection(request, "Durable Memory", FormatRequestToken(durableMemoryToken));
        return request.ToString().Trim();
    }

    private static string GetScenarioTextValue(JObject scenario, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = scenario[key]?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static JToken? GetScenarioToken(JObject scenario, params string[] keys)
    {
        foreach (var key in keys)
        {
            var token = scenario[key];
            if (token is not null && token.Type != JTokenType.Null)
            {
                return token;
            }
        }

        return null;
    }

    private static bool IsNullOrEmptyToken(JToken? token)
    {
        if (token is null || token.Type == JTokenType.Null) return true;
        if (token is JArray array) return array.Count == 0;
        if (token is JObject obj) return !obj.Properties().Any();
        if (token.Type == JTokenType.String) return string.IsNullOrWhiteSpace(token.ToString());
        return false;
    }

    private static string FormatRequestToken(JToken? token)
    {
        if (IsNullOrEmptyToken(token))
        {
            return "(none)";
        }

        var content = token!;

        return content is JValue
            ? content.ToString()
            : content.ToString(Newtonsoft.Json.Formatting.Indented);
    }

    private static void AppendRequestSection(System.Text.StringBuilder builder, string heading, string content)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine($"# {heading}");
        builder.AppendLine(content);
    }

    private static bool IsScenarioFileType(string fileType) => string.Equals(fileType, ScenarioFileType, StringComparison.OrdinalIgnoreCase);

    private static string GetMockDataFileTypeFromRelativePath(string relativePath)
    {
        var lower = relativePath.ToLowerInvariant();
        if (lower.EndsWith(".user-request.md")) return UserRequestFileType;
        if (lower.EndsWith(".application-hierarchy.json")) return ApplicationHierarchyFileType;
        if (lower.EndsWith(".completed-steps.txt")) return CompletedStepsFileType;
        if (lower.EndsWith(".durable-memory.json")) return DurableMemoryFileType;
        return ScenarioFileType;
    }

    private static string NormalizeContentForSave(string fileType, string content)
    {
        if (string.Equals(fileType, UserRequestFileType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileType, CompletedStepsFileType, StringComparison.OrdinalIgnoreCase))
        {
            return content ?? string.Empty;
        }

        var parsed = JToken.Parse(content);
        return parsed.ToString(Newtonsoft.Json.Formatting.Indented);
    }

    private static string BuildSpecialFileRelativePath(string selectedFolder, string fileType, string specialName)
    {
        var fileName = fileType switch
        {
            var x when string.Equals(x, ScenarioFileType, StringComparison.OrdinalIgnoreCase) => $"{specialName}.scenario.json",
            var x when string.Equals(x, UserRequestFileType, StringComparison.OrdinalIgnoreCase) => $"{specialName}.user-request.md",
            var x when string.Equals(x, ApplicationHierarchyFileType, StringComparison.OrdinalIgnoreCase) => $"{specialName}.application-hierarchy.json",
            var x when string.Equals(x, CompletedStepsFileType, StringComparison.OrdinalIgnoreCase) => $"{specialName}.completed-steps.txt",
            var x when string.Equals(x, DurableMemoryFileType, StringComparison.OrdinalIgnoreCase) => $"{specialName}.durable-memory.json",
            _ => $"{specialName}.scenario.json"
        };

        return $"{selectedFolder}/{fileName}";
    }

    private static string BuildNewFileTemplate(string fileType, string specialName)
    {
        return fileType switch
        {
            var x when string.Equals(x, ScenarioFileType, StringComparison.OrdinalIgnoreCase) => string.Join(Environment.NewLine,
                "{",
                "  \"Name\": \"New Scenario\",",
                "  \"Goal\": \"Describe the automation goal.\",",
                "  \"SurfaceType\": \"Windows\",",
                "  \"TaskPrefix\": \"\",",
                $"  \"UserRequestFile\": \"{specialName}.user-request.md\",",
                $"  \"ApplicationHierarchyFile\": \"{specialName}.application-hierarchy.json\",",
                $"  \"CompletedStepsFile\": \"{specialName}.completed-steps.txt\",",
                $"  \"DurableMemoryFile\": \"{specialName}.durable-memory.json\"",
                "}"),
            var x when string.Equals(x, UserRequestFileType, StringComparison.OrdinalIgnoreCase) => "Describe the user request for this scenario.",
            var x when string.Equals(x, ApplicationHierarchyFileType, StringComparison.OrdinalIgnoreCase) => string.Join(Environment.NewLine,
                "{",
                "  \"ApplicationName\": \"\",",
                "  \"Technology\": \"Windows\",",
                "  \"TopLevelContainers\": []",
                "}"),
            var x when string.Equals(x, CompletedStepsFileType, StringComparison.OrdinalIgnoreCase) => string.Join(Environment.NewLine,
                "navigation: Opened target application",
                "navigation: Reached working screen"),
            var x when string.Equals(x, DurableMemoryFileType, StringComparison.OrdinalIgnoreCase) => string.Join(Environment.NewLine,
                "{",
                "  \"automationSignatures\": [],",
                "  \"notes\": []",
                "}"),
            _ => string.Empty
        };
    }

    private static string NormalizeSpecialFileName(string value)
    {
        var raw = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw)) raw = "new-scenario";

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            raw = raw.Replace(invalid, '-');
        }

        raw = raw.Replace(' ', '-').Trim('-');
        return string.IsNullOrWhiteSpace(raw) ? "new-scenario" : raw;
    }

    private static string ExtractSpecialFileNameFromRelativePath(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(fileName)) return "new-scenario";

        var knownSuffixes = new[]
        {
            ".scenario.json",
            ".user-request.md",
            ".application-hierarchy.json",
            ".completed-steps.txt",
            ".durable-memory.json",
            ".json",
            ".md",
            ".txt"
        };

        foreach (var suffix in knownSuffixes)
        {
            if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                var value = fileName[..^suffix.Length];
                return string.IsNullOrWhiteSpace(value) ? "new-scenario" : value;
            }
        }

        return fileName;
    }

    private void SyncMockDataRelativePathFromSelection()
    {
        if (_plannerPackage is null) return;

        var selectedFolder = string.IsNullOrWhiteSpace(SelectedUseCaseFolder) ? "mock-data" : SelectedUseCaseFolder.TrimEnd('/');
        var specialName = NormalizeSpecialFileName(_specialFileName);
        if (!string.Equals(_specialFileName, specialName, StringComparison.Ordinal))
        {
            _specialFileName = specialName;
            OnPropertyChanged(nameof(SpecialFileName));
        }

        var nextRelativePath = BuildSpecialFileRelativePath(selectedFolder, SelectedMockDataFileType, specialName);
        if (!string.Equals(_mockDataRelativePath, nextRelativePath, StringComparison.OrdinalIgnoreCase))
        {
            _mockDataRelativePath = nextRelativePath;
            OnPropertyChanged(nameof(MockDataRelativePath));
        }
    }

    private string ResolveLinkedMockDataFilePath(ScenarioDocument scenario, string linkedPath, string fieldName)
    {
        if (_plannerPackage is null) throw new InvalidOperationException("Load a planner package before resolving linked mock-data files.");

        var normalizedLinkedPath = (linkedPath ?? string.Empty).Trim().Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalizedLinkedPath))
        {
            throw new InvalidOperationException($"{fieldName} cannot be blank.");
        }

        var mockDataRoot = Path.GetFullPath(_plannerPackage.MockDataRootPath);
        var attemptedPaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(scenario.RelativePath))
        {
            var scenarioRelative = NormalizeMockDataRelativePath(scenario.RelativePath);
            var scenarioDirectory = Path.GetDirectoryName(scenarioRelative.Replace('/', Path.DirectorySeparatorChar));
            if (!string.IsNullOrWhiteSpace(scenarioDirectory))
            {
                var scenarioFolderCandidate = Path.GetFullPath(Path.Combine(mockDataRoot, scenarioDirectory, normalizedLinkedPath.Replace('/', Path.DirectorySeparatorChar)));
                attemptedPaths.Add(scenarioFolderCandidate);
            }
        }

        var rootCandidate = Path.GetFullPath(Path.Combine(mockDataRoot, normalizedLinkedPath.Replace('/', Path.DirectorySeparatorChar)));
        attemptedPaths.Add(rootCandidate);

        foreach (var candidate in attemptedPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!IsPathUnderRoot(mockDataRoot, candidate)) continue;
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException($"Could not resolve {fieldName} file '{linkedPath}' under mock-data root '{mockDataRoot}'.");
    }

    private static bool IsPathUnderRoot(string rootPath, string fullPath)
    {
        var normalizedRoot = Path.GetFullPath(rootPath);
        var normalizedFullPath = Path.GetFullPath(fullPath);
        var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar) ? normalizedRoot : normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedFullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static ScenarioDocument ParseScenario(string name, string json, string? relativePath = null)
    {
        return new ScenarioDocument { Name = name, RelativePath = relativePath ?? string.Empty, Json = json, Parsed = JObject.Parse(json) };
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
            throw new InvalidOperationException("Enter a mock-data file name before saving.");
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
