using System.Windows;
using AutomationPlanner.POC.ViewModels;

namespace AutomationPlanner.POC;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => ApiKeyBox.Password = ((SettingsViewModel)DataContext).Settings.OpenAiApiKey;
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm) vm.Settings.OpenAiApiKey = ApiKeyBox.Password;
    }

    private void BrowsePlannerPackage_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && TryBrowseFolder("Select automation planner package folder", out var path))
        {
            vm.Settings.PlannerPackagePath = path;
            vm.OnSettingsChanged();
        }
    }

    private void BrowseMockData_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && TryBrowseFolder("Select mock data base folder", out var path))
        {
            vm.Settings.MockDataBasePath = path;
            vm.OnSettingsChanged();
        }
    }

    private static bool TryBrowseFolder(string description, out string path)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = description };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            path = dialog.SelectedPath;
            return true;
        }

        path = string.Empty;
        return false;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            await vm.SaveAsync();
        }

        DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
