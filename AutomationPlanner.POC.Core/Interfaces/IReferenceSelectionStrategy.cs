using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IReferenceSelectionStrategy
{
    IReadOnlyDictionary<string, string> SelectReferences(PlannerPackage package, ScenarioDocument scenario);
}
