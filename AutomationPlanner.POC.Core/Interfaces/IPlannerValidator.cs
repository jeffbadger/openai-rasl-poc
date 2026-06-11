using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Core.Interfaces;

public interface IPlannerValidator
{
    PlannerValidationResult Validate(string responseText);
}
