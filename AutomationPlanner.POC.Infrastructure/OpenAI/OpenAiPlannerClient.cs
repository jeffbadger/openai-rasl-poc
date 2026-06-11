using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.OpenAI;
using AutomationPlanner.POC.Models.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutomationPlanner.POC.Infrastructure.OpenAI;

public sealed class OpenAiPlannerClient(HttpClient? httpClient = null) : IOpenAiPlannerClient
{
    private const int MaxToolTurns = 5;
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };

    public async Task<OpenAiPlannerResult> CreatePlanAsync(string prompt, AppSettings settings, IMockAutomationRuntime automationRuntime, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.OpenAiApiKey)) throw new InvalidOperationException("OpenAI API key is not configured.");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, settings.RequestTimeoutSeconds)));

        var inputItems = new List<JToken>
        {
            JObject.FromObject(new OpenAiInputMessage
            {
                Role = "user",
                Content = [new OpenAiInputContent { Text = prompt }]
            })
        };

        var stopwatch = Stopwatch.StartNew();
        var requestLog = new JArray();
        var responseLog = new JArray();
        var totalInputTokens = 0;
        var totalOutputTokens = 0;
        OpenAiResponsesEnvelope? finalEnvelope = null;

        for (var turn = 1; turn <= MaxToolTurns; turn++)
        {
            var requestModel = CreateRequestModel(inputItems, settings);
            var rawRequest = JsonConvert.SerializeObject(requestModel, Formatting.Indented);
            requestLog.Add(JObject.Parse(rawRequest));

            var rawResponse = await SendWithRetriesAsync(rawRequest, settings.OpenAiApiKey, timeoutCts.Token);
            responseLog.Add(JToken.Parse(rawResponse));

            var envelope = JsonConvert.DeserializeObject<OpenAiResponsesEnvelope>(rawResponse) ?? new OpenAiResponsesEnvelope();
            totalInputTokens += envelope.Usage?["input_tokens"]?.Value<int?>() ?? 0;
            totalOutputTokens += envelope.Usage?["output_tokens"]?.Value<int?>() ?? 0;
            finalEnvelope = envelope;

            var functionCalls = GetFunctionCalls(envelope).ToList();
            if (functionCalls.Count == 0) break;

            if (turn == MaxToolTurns)
            {
                throw new InvalidOperationException($"OpenAI Responses API did not produce a final answer after {MaxToolTurns} tool turns.");
            }

            if (envelope.Output is not null)
            {
                inputItems.AddRange(envelope.Output.Select(item => item.DeepClone()));
            }

            foreach (var functionCall in functionCalls)
            {
                inputItems.Add(await CreateFunctionCallOutputAsync(functionCall, automationRuntime, timeoutCts.Token));
            }
        }

        stopwatch.Stop();
        if (finalEnvelope is null) throw new InvalidOperationException("OpenAI request did not produce a response.");

        return new OpenAiPlannerResult
        {
            RawRequest = FormatTurnLog(requestLog),
            RawResponse = FormatTurnLog(responseLog),
            OutputText = ExtractOutputText(finalEnvelope),
            Duration = stopwatch.Elapsed,
            InputTokens = totalInputTokens == 0 ? null : totalInputTokens,
            OutputTokens = totalOutputTokens == 0 ? null : totalOutputTokens
        };
    }

    private static OpenAiPlannerRequest CreateRequestModel(List<JToken> inputItems, AppSettings settings)
    {
        return new OpenAiPlannerRequest
        {
            Model = string.IsNullOrWhiteSpace(settings.Model) ? "gpt-5.2" : settings.Model,
            Temperature = settings.Temperature,
            MaxOutputTokens = settings.MaxTokens,
            Tools = [CreateAskUserToolDefinition()],
            ParallelToolCalls = false,
            Input = inputItems.Select(item => item.DeepClone()).ToList()
        };
    }

    private async Task<string> SendWithRetriesAsync(string rawRequest, string apiKey, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        string rawResponse = string.Empty;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(rawRequest, Encoding.UTF8, "application/json");

            response = await _httpClient.SendAsync(request, cancellationToken);
            rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode) break;
            if (!ShouldRetry(response.StatusCode) || attempt == 3) break;
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
        }

        if (response is null) throw new InvalidOperationException("OpenAI request did not produce a response.");
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"OpenAI Responses API failed with {(int)response.StatusCode}: {rawResponse}");

        return rawResponse;
    }

    private static OpenAiToolDefinition CreateAskUserToolDefinition()
    {
        return new OpenAiToolDefinition
        {
            Name = "ask_user",
            Description = "Ask the application user a concise clarification question when the automation plan cannot proceed without the answer.",
            Strict = true,
            Parameters = new JObject
            {
                ["type"] = "object",
                ["properties"] = new JObject
                {
                    ["question"] = new JObject
                    {
                        ["type"] = "string",
                        ["description"] = "The clarification question to show to the user."
                    }
                },
                ["required"] = new JArray("question"),
                ["additionalProperties"] = false
            }
        };
    }

    private static IEnumerable<JObject> GetFunctionCalls(OpenAiResponsesEnvelope envelope)
    {
        if (envelope.Output is null) yield break;

        foreach (var item in envelope.Output.OfType<JObject>())
        {
            if (string.Equals(item["type"]?.ToString(), "function_call", StringComparison.OrdinalIgnoreCase))
            {
                yield return item;
            }
        }
    }

    private static async Task<JObject> CreateFunctionCallOutputAsync(JObject functionCall, IMockAutomationRuntime automationRuntime, CancellationToken cancellationToken)
    {
        var name = functionCall["name"]?.ToString() ?? string.Empty;
        var callId = functionCall["call_id"]?.ToString();
        if (string.IsNullOrWhiteSpace(callId)) throw new InvalidOperationException("OpenAI function call did not include a call_id.");

        var arguments = ParseArguments(functionCall["arguments"]?.ToString());
        JToken output = string.Equals(name, "ask_user", StringComparison.OrdinalIgnoreCase)
            ? new JObject
            {
                ["answer"] = await automationRuntime.AskUserAsync(arguments["question"]?.ToString() ?? string.Empty, cancellationToken)
            }
            : await automationRuntime.InvokeToolAsync(name, arguments, cancellationToken);

        return new JObject
        {
            ["type"] = "function_call_output",
            ["call_id"] = callId,
            ["output"] = output.ToString(Formatting.None)
        };
    }

    private static JObject ParseArguments(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments)) return new JObject();

        var parsed = JToken.Parse(arguments);
        return parsed as JObject ?? new JObject { ["value"] = parsed };
    }

    private static string FormatTurnLog(JArray turns)
    {
        return turns.Count == 1 ? turns[0]!.ToString(Formatting.Indented) : turns.ToString(Formatting.Indented);
    }

    private static bool ShouldRetry(HttpStatusCode code) => code == HttpStatusCode.TooManyRequests || (int)code >= 500;

    private static string ExtractOutputText(OpenAiResponsesEnvelope envelope)
    {
        if (!string.IsNullOrWhiteSpace(envelope.OutputText)) return envelope.OutputText!;
        if (envelope.Output is null) return string.Empty;

        var parts = envelope.Output
            .SelectMany(item => item["content"] as JArray ?? new JArray())
            .Select(content => content["text"]?.ToString())
            .Where(text => !string.IsNullOrWhiteSpace(text));
        return string.Join(Environment.NewLine, parts);
    }
}
