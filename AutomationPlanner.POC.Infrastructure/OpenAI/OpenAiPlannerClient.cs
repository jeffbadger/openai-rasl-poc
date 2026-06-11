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
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };

    public async Task<OpenAiPlannerResult> CreatePlanAsync(string prompt, AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.OpenAiApiKey)) throw new InvalidOperationException("OpenAI API key is not configured.");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, settings.RequestTimeoutSeconds)));

        var requestModel = new OpenAiPlannerRequest
        {
            Model = string.IsNullOrWhiteSpace(settings.Model) ? "gpt-5.2" : settings.Model,
            Temperature = settings.Temperature,
            MaxOutputTokens = settings.MaxTokens,
            Input =
            [
                new OpenAiInputMessage
                {
                    Role = "user",
                    Content = [new OpenAiInputContent { Text = prompt }]
                }
            ]
        };

        var rawRequest = JsonConvert.SerializeObject(requestModel, Formatting.Indented);
        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage? response = null;
        string rawResponse = string.Empty;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenAiApiKey);
            request.Content = new StringContent(rawRequest, Encoding.UTF8, "application/json");

            response = await _httpClient.SendAsync(request, timeoutCts.Token);
            rawResponse = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            if (response.IsSuccessStatusCode) break;
            if (!ShouldRetry(response.StatusCode) || attempt == 3) break;
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), timeoutCts.Token);
        }

        stopwatch.Stop();
        if (response is null) throw new InvalidOperationException("OpenAI request did not produce a response.");
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"OpenAI Responses API failed with {(int)response.StatusCode}: {rawResponse}");

        var envelope = JsonConvert.DeserializeObject<OpenAiResponsesEnvelope>(rawResponse) ?? new OpenAiResponsesEnvelope();
        return new OpenAiPlannerResult
        {
            RawRequest = rawRequest,
            RawResponse = rawResponse,
            OutputText = ExtractOutputText(envelope),
            Duration = stopwatch.Elapsed,
            InputTokens = envelope.Usage?["input_tokens"]?.Value<int?>(),
            OutputTokens = envelope.Usage?["output_tokens"]?.Value<int?>()
        };
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
