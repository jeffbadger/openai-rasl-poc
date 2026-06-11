using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IPlannerPackageLoader
{
    Task<PlannerPackage> LoadAsync(string rootPath, CancellationToken cancellationToken = default);
}
