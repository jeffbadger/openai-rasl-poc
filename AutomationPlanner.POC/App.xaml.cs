using System.Net.Http;
using System.Windows;
using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Infrastructure.OpenAI;
using AutomationPlanner.POC.Services.Export;
using AutomationPlanner.POC.Services.Loading;
using AutomationPlanner.POC.Services.Prompting;
using AutomationPlanner.POC.Services.Runtime;
using AutomationPlanner.POC.Services.Settings;
using AutomationPlanner.POC.Services.Validation;
using AutomationPlanner.POC.ViewModels;

namespace AutomationPlanner.POC;

public partial class App : System.Windows.Application
{
    public MainViewModel MainViewModel { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        IPlannerPackageLoader loader = new PlannerPackageLoader();
        IReferenceSelectionStrategy references = new LoadAllReferenceSelectionStrategy();
        IPromptAssembler assembler = new PromptAssembler(references);
        IOpenAiPlannerClient client = new OpenAiPlannerClient(new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") });
        IPlannerValidator validator = new PlannerValidator();
        IMockAutomationRuntime mockRuntime = new MockAutomationRuntime();
        ISettingsStore settings = new JsonSettingsStore();
        IExportService export = new ExportService();
        MainViewModel = new MainViewModel(loader, assembler, client, validator, mockRuntime, settings, export);
        await MainViewModel.InitializeAsync();
        var window = new MainWindow { DataContext = MainViewModel };
        window.Show();
    }
}
