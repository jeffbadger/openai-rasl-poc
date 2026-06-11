using System.Windows;
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
        var window = new SettingsWindow { Owner = this, DataContext = ViewModel.SettingsViewModel };
        window.ShowDialog();
        ViewModel.Settings = ViewModel.SettingsViewModel.Settings;
    }
}
