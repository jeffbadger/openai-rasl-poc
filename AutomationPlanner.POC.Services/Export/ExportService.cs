using AutomationPlanner.POC.Core.Interfaces;

namespace AutomationPlanner.POC.Services.Export;

public sealed class ExportService : IExportService
{
    public async Task SaveTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }
}
