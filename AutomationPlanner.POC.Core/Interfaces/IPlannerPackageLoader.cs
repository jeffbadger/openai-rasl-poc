using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IPlannerPackageLoader
{
    Task<PlannerPackage> LoadAsync(string rootPath, string? mockDataBasePath = null, CancellationToken cancellationToken = default);
}
