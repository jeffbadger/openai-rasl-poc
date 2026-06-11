using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Settings;
using AutomationPlanner.POC.ViewModels.Commands;
using System.Windows.Input;

namespace AutomationPlanner.POC.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private AppSettings _settings = new();

    public SettingsViewModel(ISettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public ICommand SaveCommand { get; }
    public AppSettings Settings { get => _settings; set => SetProperty(ref _settings, value); }

    public void OnSettingsChanged() => OnPropertyChanged(nameof(Settings));

    public async Task LoadAsync() => Settings = await _settingsStore.LoadAsync();
    public Task SaveAsync() => _settingsStore.SaveAsync(Settings);
}
