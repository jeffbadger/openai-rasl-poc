namespace AutomationPlanner.POC.Core.Interfaces;

public interface IExportService
{
    Task SaveTextAsync(string path, string content, CancellationToken cancellationToken = default);
}
