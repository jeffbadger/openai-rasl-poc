using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.ViewModels;

public sealed class ScenarioItemViewModel(ScenarioDocument scenario)
{
    public ScenarioDocument Scenario { get; } = scenario;
    public string Name => Scenario.Name;
    public string RelativePath => Scenario.RelativePath;
    public override string ToString() => Name;
}
