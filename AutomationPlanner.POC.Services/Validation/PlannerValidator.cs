using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Planner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Services.Validation;

public sealed class PlannerValidator : IPlannerValidator
{
    private static readonly string[] RequiredTopLevel =
    [
        "AutomationName", "AutomationDescription", "AutomationContext", "AutomationCategory", "Steps", "GoalCompleted", "CompletedStepSummaries"
    ];

    public PlannerValidationResult Validate(string responseText)
    {
        var result = new PlannerValidationResult();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            result.Errors.Add("Response is empty.");
            return result;
        }

        var json = ExtractJson(responseText);
        JObject root;
        try
        {
            root = JObject.Parse(json);
            result.NormalizedJson = root.ToString(Formatting.Indented);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Response must be valid JSON: {ex.Message}");
            return result;
        }

        foreach (var property in RequiredTopLevel)
        {
            if (root[property] is null) result.Errors.Add($"Missing required property: {property}.");
        }

        if (root["Steps"] is not JArray steps)
        {
            result.Errors.Add("Steps must be an array.");
        }
        else
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] is not JObject step)
                {
                    result.Errors.Add($"Steps[{i}] must be an object.");
                    continue;
                }
                if (step["StepDescription"] is null || string.IsNullOrWhiteSpace(step["StepDescription"]?.ToString()))
                {
                    result.Errors.Add($"Steps[{i}] is missing StepDescription.");
                }
            }
        }

        if (root["CompletedStepSummaries"] is not JArray) result.Errors.Add("CompletedStepSummaries must be an array.");
        return result;
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewLine >= 0 && lastFence > firstNewLine) return trimmed[(firstNewLine + 1)..lastFence].Trim();
        }
        return trimmed;
    }
}
