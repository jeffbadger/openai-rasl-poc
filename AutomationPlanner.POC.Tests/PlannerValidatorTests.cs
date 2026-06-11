using AutomationPlanner.POC.Services.Validation;
using Xunit;

namespace AutomationPlanner.POC.Tests;

public sealed class PlannerValidatorTests
{
    [Fact]
    public void Validate_Accepts_MinimalPlannerContract()
    {
        var validator = new PlannerValidator();
        var json = """
        {
          "AutomationName": "Demo",
          "AutomationDescription": "Demo plan",
          "AutomationContext": {},
          "AutomationCategory": "Web",
          "Steps": [{ "StepType": "Method", "StepDescription": "Do the first thing" }],
          "GoalCompleted": false,
          "CompletedStepSummaries": []
        }
        """;

        var result = validator.Validate(json);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_Rejects_MissingStepDescription()
    {
        var validator = new PlannerValidator();
        var json = """
        {
          "AutomationName": "Demo",
          "AutomationDescription": "Demo plan",
          "AutomationContext": {},
          "AutomationCategory": "Web",
          "Steps": [{ "StepType": "Method" }],
          "GoalCompleted": false,
          "CompletedStepSummaries": []
        }
        """;

        var result = validator.Validate(json);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("StepDescription"));
    }
}
