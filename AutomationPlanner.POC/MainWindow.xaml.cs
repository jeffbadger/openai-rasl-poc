using System.Windows;
using AutomationPlanner.POC.Models.Settings;
using AutomationPlanner.POC.ViewModels;

namespace AutomationPlanner.POC;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is null) DataContext = ((App)System.Windows.Application.Current).MainViewModel;
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "Select planner package folder" };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) ViewModel.SelectedPlannerRoot = dialog.SelectedPath;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SettingsViewModel.Settings = CloneSettings(ViewModel.Settings);
        var window = new SettingsWindow { Owner = this, DataContext = ViewModel.SettingsViewModel };
        if (window.ShowDialog() == true)
        {
            ViewModel.Settings = ViewModel.SettingsViewModel.Settings;
            if (!string.IsNullOrWhiteSpace(ViewModel.Settings.PlannerPackagePath))
            {
                ViewModel.SelectedPlannerRoot = ViewModel.Settings.PlannerPackagePath;
            }
        }
    }

    private static AppSettings CloneSettings(AppSettings settings) => new()
    {
        OpenAiApiKey = settings.OpenAiApiKey,
        Model = settings.Model,
        Temperature = settings.Temperature,
        MaxTokens = settings.MaxTokens,
        RequestTimeoutSeconds = settings.RequestTimeoutSeconds,
        PlannerPackagePath = settings.PlannerPackagePath,
        MockDataBasePath = settings.MockDataBasePath,
        LastPlannerPackagePath = settings.LastPlannerPackagePath
    };
}
