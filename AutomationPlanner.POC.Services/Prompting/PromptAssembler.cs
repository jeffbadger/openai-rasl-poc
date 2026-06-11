using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Services.Prompting;

public sealed class PromptAssembler(IReferenceSelectionStrategy referenceSelectionStrategy) : IPromptAssembler
{
    private const string SystemHeader = "You are an automation planner. Return only the planner JSON contract defined by the loaded planner package.";

    public PromptAssembly Assemble(PlannerPackage package, ScenarioDocument scenario, string userRequest)
    {
        var references = referenceSelectionStrategy.SelectReferences(package, scenario);
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("# System Header");
        builder.AppendLine(SystemHeader);
        builder.AppendLine();
        builder.AppendLine("# Loaded SKILL.md");
        builder.AppendLine(package.SkillContent);
        builder.AppendLine();
        builder.AppendLine("# Required Reference Files");
        foreach (var reference in references.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"\n## {reference.Key}");
            builder.AppendLine(reference.Value);
        }
        builder.AppendLine("\n# Scenario JSON");
        builder.AppendLine(scenario.Json);
        builder.AppendLine("\n# User Request");
        builder.AppendLine(userRequest);

        var prompt = builder.ToString();
        return new PromptAssembly
        {
            SystemHeader = SystemHeader,
            AssembledPrompt = prompt,
            IncludedReferences = references.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
            EstimatedTokens = EstimateTokens(prompt)
        };
    }

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
}
