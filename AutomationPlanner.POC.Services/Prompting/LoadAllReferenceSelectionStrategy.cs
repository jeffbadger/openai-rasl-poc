using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Services.Prompting;

public sealed class LoadAllReferenceSelectionStrategy : IReferenceSelectionStrategy
{
    public IReadOnlyDictionary<string, string> SelectReferences(PlannerPackage package, ScenarioDocument scenario) => package.ReferenceFiles;
}
