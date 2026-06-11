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

    private void Save_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
