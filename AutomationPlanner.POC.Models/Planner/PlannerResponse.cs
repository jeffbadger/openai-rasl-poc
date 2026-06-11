using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Models.Planner;

public sealed class PlannerResponse
{
    public string AutomationName { get; set; } = string.Empty;
    public string AutomationDescription { get; set; } = string.Empty;
    public JObject AutomationContext { get; set; } = new();
    public string AutomationCategory { get; set; } = string.Empty;
    public List<PlannerStep> Steps { get; set; } = [];
    public bool GoalCompleted { get; set; }
    public List<string> CompletedStepSummaries { get; set; } = [];
}

[JsonConverter(typeof(PlannerStepConverter))]
public abstract class PlannerStep
{
    public string StepType { get; set; } = string.Empty;
    public string StepDescription { get; set; } = string.Empty;
}

public sealed class DecisionStep : PlannerStep
{
    public string Condition { get; set; } = string.Empty;
    public List<PlannerStep> WhenTrue { get; set; } = [];
    public List<PlannerStep> WhenFalse { get; set; } = [];
}

public sealed class MethodStep : PlannerStep
{
    public string MethodName { get; set; } = string.Empty;
    public JObject Arguments { get; set; } = new();
}

public sealed class ApplicationStep : PlannerStep
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Selector { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public JObject Parameters { get; set; } = new();
}

public sealed class LoopStep : PlannerStep
{
    public string LoopKind { get; set; } = string.Empty;
    public string Iterator { get; set; } = string.Empty;
    public List<PlannerStep> Body { get; set; } = [];
}

public sealed class LabelStep : PlannerStep
{
    public string Label { get; set; } = string.Empty;
}

public sealed class TodoStep : PlannerStep
{
    public string TodoReason { get; set; } = string.Empty;
}

public sealed class PlannerStepConverter : JsonConverter<PlannerStep>
{
    public override PlannerStep? ReadJson(JsonReader reader, Type objectType, PlannerStep? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var stepType = obj["StepType"]?.ToString() ?? obj["Type"]?.ToString() ?? string.Empty;
        PlannerStep target = stepType.ToLowerInvariant() switch
        {
            "decision" or "decisionstep" => new DecisionStep(),
            "method" or "methodstep" => new MethodStep(),
            "application" or "applicationstep" or "applicationmethod" or "applicationvalue" => new ApplicationStep(),
            "loop" or "loopstep" or "for" or "while" or "dowhile" or "listloop" => new LoopStep(),
            "label" or "labelstep" => new LabelStep(),
            "todo" or "todostep" => new TodoStep(),
            _ => new MethodStep { StepType = string.IsNullOrWhiteSpace(stepType) ? "Unknown" : stepType }
        };

        using var objectReader = obj.CreateReader();
        serializer.Populate(objectReader, target);
        return target;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, PlannerStep? value, JsonSerializer serializer) => throw new NotSupportedException();
}
