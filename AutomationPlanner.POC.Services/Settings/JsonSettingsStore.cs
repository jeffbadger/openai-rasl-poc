using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Settings;
using Newtonsoft.Json;

namespace AutomationPlanner.POC.Services.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string _settingsPath;

    public JsonSettingsStore(string? settingsPath = null)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = settingsPath ?? Path.Combine(appData, "AutomationPlanner.POC", "settings.json");
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath)) return new AppSettings();
        var json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
        return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        await File.WriteAllTextAsync(_settingsPath, json, cancellationToken);
    }
}
