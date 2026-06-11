using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Models.OpenAI;

public sealed class OpenAiPlannerRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = "gpt-5.2";

    [JsonProperty("input")]
    public List<JToken> Input { get; set; } = [];

    [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
    public List<OpenAiToolDefinition>? Tools { get; set; }

    [JsonProperty("parallel_tool_calls", NullValueHandling = NullValueHandling.Ignore)]
    public bool? ParallelToolCalls { get; set; }

    [JsonProperty("temperature", NullValueHandling = NullValueHandling.Ignore)]
    public double? Temperature { get; set; }

    [JsonProperty("max_output_tokens", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxOutputTokens { get; set; }
}

public sealed class OpenAiInputMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = "user";

    [JsonProperty("content")]
    public List<OpenAiInputContent> Content { get; set; } = [];
}

public sealed class OpenAiInputContent
{
    [JsonProperty("type")]
    public string Type { get; set; } = "input_text";

    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;
}

public sealed class OpenAiToolDefinition
{
    [JsonProperty("type")]
    public string Type { get; set; } = "function";

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("parameters")]
    public JObject Parameters { get; set; } = new();

    [JsonProperty("strict", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Strict { get; set; }
}

public sealed class OpenAiPlannerResult
{
    public string RawRequest { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public string OutputText { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
}

public sealed class OpenAiResponsesEnvelope
{
    [JsonProperty("output_text")]
    public string? OutputText { get; set; }

    [JsonProperty("output")]
    public JArray? Output { get; set; }

    [JsonProperty("usage")]
    public JObject? Usage { get; set; }
}
