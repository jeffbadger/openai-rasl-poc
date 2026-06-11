using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IPromptAssembler
{
    PromptAssembly Assemble(PlannerPackage package, ScenarioDocument scenario, string userRequest);
}
