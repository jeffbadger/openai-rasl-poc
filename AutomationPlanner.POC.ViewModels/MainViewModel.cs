using System.Collections.ObjectModel;
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
    private readonly ISettingsStore _settingsStore;
    private readonly IExportService _exportService;
    private PlannerPackage? _plannerPackage;
    private ScenarioItemViewModel? _selectedScenario;
    private string _selectedPlannerRoot = string.Empty;
    private string _scenarioJson = string.Empty;
    private string _promptAssembly = string.Empty;
    private string _rawRequest = string.Empty;
    private string _rawResponse = string.Empty;
    private string _plannerJson = string.Empty;
    private string _validationText = string.Empty;
    private string _consoleText = string.Empty;
    private string _diagnosticsText = string.Empty;
    private AppSettings _settings = new();

    public MainViewModel(IPlannerPackageLoader packageLoader, IPromptAssembler promptAssembler, IOpenAiPlannerClient plannerClient, IPlannerValidator validator, ISettingsStore settingsStore, IExportService exportService)
    {
        _packageLoader = packageLoader;
        _promptAssembler = promptAssembler;
        _plannerClient = plannerClient;
        _validator = validator;
        _settingsStore = settingsStore;
        _exportService = exportService;
        ReloadPlannerCommand = new AsyncRelayCommand(ReloadPlannerAsync, () => !string.IsNullOrWhiteSpace(SelectedPlannerRoot));
        LoadScenarioCommand = new RelayCommand(LoadSelectedScenario, () => SelectedScenario is not null);
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync, () => _plannerPackage is not null && !string.IsNullOrWhiteSpace(ScenarioJson));
        RunAllCommand = new AsyncRelayCommand(RunAllAsync, () => _plannerPackage is not null && Scenarios.Count > 0);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !string.IsNullOrWhiteSpace(PromptAssembly));
        SettingsViewModel = new SettingsViewModel(settingsStore);
    }

    public ObservableCollection<string> PackageTree { get; } = [];
    public ObservableCollection<ScenarioItemViewModel> Scenarios { get; } = [];
    public SettingsViewModel SettingsViewModel { get; }
    public ICommand ReloadPlannerCommand { get; }
    public ICommand LoadScenarioCommand { get; }
    public ICommand ExecuteCommand { get; }
    public ICommand RunAllCommand { get; }
    public ICommand ExportCommand { get; }

    public string SelectedPlannerRoot { get => _selectedPlannerRoot; set { if (SetProperty(ref _selectedPlannerRoot, value)) RaiseCommands(); } }
    public ScenarioItemViewModel? SelectedScenario { get => _selectedScenario; set { if (SetProperty(ref _selectedScenario, value)) RaiseCommands(); } }
    public string ScenarioJson { get => _scenarioJson; set { if (SetProperty(ref _scenarioJson, value)) RaiseCommands(); } }
    public string PromptAssembly { get => _promptAssembly; set => SetProperty(ref _promptAssembly, value); }
    public string RawRequest { get => _rawRequest; set => SetProperty(ref _rawRequest, value); }
    public string RawResponse { get => _rawResponse; set => SetProperty(ref _rawResponse, value); }
    public string PlannerJson { get => _plannerJson; set => SetProperty(ref _plannerJson, value); }
    public string ValidationText { get => _validationText; set => SetProperty(ref _validationText, value); }
    public string ConsoleText { get => _consoleText; set => SetProperty(ref _consoleText, value); }
    public string DiagnosticsText { get => _diagnosticsText; set => SetProperty(ref _diagnosticsText, value); }
    public AppSettings Settings { get => _settings; set => SetProperty(ref _settings, value); }

    public async Task InitializeAsync()
    {
        Settings = await _settingsStore.LoadAsync();
        SettingsViewModel.Settings = Settings;
        if (!string.IsNullOrWhiteSpace(Settings.LastPlannerPackagePath)) SelectedPlannerRoot = Settings.LastPlannerPackagePath;
    }

    public async Task ReloadPlannerAsync()
    {
        AppendConsole($"Loading planner package: {SelectedPlannerRoot}");
        _plannerPackage = await _packageLoader.LoadAsync(SelectedPlannerRoot);
        Settings.LastPlannerPackagePath = SelectedPlannerRoot;
        await _settingsStore.SaveAsync(Settings);
        RefreshPackageTree();
        DiscoverScenarios();
        UpdateDiagnostics();
        AppendConsole($"Loaded {_plannerPackage.ReferenceFiles.Count} references and {Scenarios.Count} scenarios.");
        RaiseCommands();
    }

    public void LoadSelectedScenario()
    {
        if (SelectedScenario is null) return;
        ScenarioJson = SelectedScenario.Scenario.Json;
        AppendConsole($"Loaded scenario: {SelectedScenario.RelativePath}");
    }

    public async Task ExecuteAsync()
    {
        if (_plannerPackage is null) return;
        var scenario = ParseScenario("Ad hoc scenario", ScenarioJson);
        var assembly = _promptAssembler.Assemble(_plannerPackage, scenario, "Create an automation plan for the supplied scenario.");
        PromptAssembly = assembly.AssembledPrompt;
        UpdateDiagnostics(assembly);
        AppendConsole($"Prompt assembled. Estimated tokens: {assembly.EstimatedTokens}.");

        OpenAiPlannerResult result = await _plannerClient.CreatePlanAsync(assembly.AssembledPrompt, Settings);
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
        foreach (var mock in _plannerPackage.MockDataFiles.Keys) PackageTree.Add("  " + mock);
    }

    private void DiscoverScenarios()
    {
        Scenarios.Clear();
        if (_plannerPackage is null) return;
        foreach (var file in _plannerPackage.MockDataFiles.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
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
        SelectedScenario = Scenarios.FirstOrDefault();
    }

    private static ScenarioDocument ParseScenario(string name, string json)
    {
        return new ScenarioDocument { Name = name, Json = json, Parsed = JObject.Parse(json) };
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
        (ExecuteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (RunAllCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ExportCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
