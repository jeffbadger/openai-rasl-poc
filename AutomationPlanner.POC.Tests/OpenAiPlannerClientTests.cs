using System.Net;
using System.Text;
using AutomationPlanner.POC.Infrastructure.OpenAI;
using AutomationPlanner.POC.Models.Settings;
using AutomationPlanner.POC.Services.Runtime;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AutomationPlanner.POC.Tests;

public sealed class OpenAiPlannerClientTests
{
    [Fact]
    public async Task CreatePlanAsync_HandlesAskUserFunctionCallWithRuntimeAnswer()
    {
        var handler = new QueueHttpMessageHandler(
            """
            {
              "output": [
                {
                  "type": "function_call",
                  "call_id": "call_ask_1",
                  "name": "ask_user",
                  "arguments": "{\"question\":\"Which queue should be processed?\"}"
                }
              ],
              "usage": { "input_tokens": 10, "output_tokens": 5 }
            }
            """,
            """
            {
              "output_text": "{\"Steps\":[]}",
              "output": [
                {
                  "type": "message",
                  "content": [
                    { "type": "output_text", "text": "{\"Steps\":[]}" }
                  ]
                }
              ],
              "usage": { "input_tokens": 12, "output_tokens": 8 }
            }
            """);
        var client = new OpenAiPlannerClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.openai.com/v1/") });
        var runtime = new MockAutomationRuntime();
        runtime.SetAskUserDefaultResponse("Process the escalations queue.");

        var result = await client.CreatePlanAsync("Create a plan.", new AppSettings { OpenAiApiKey = "test-key" }, runtime);

        Assert.Equal("{\"Steps\":[]}", result.OutputText);
        Assert.Equal(22, result.InputTokens);
        Assert.Equal(13, result.OutputTokens);
        Assert.Equal(2, handler.RequestBodies.Count);
        Assert.Contains("\"name\": \"ask_user\"", handler.RequestBodies[0]);
        Assert.Contains("\"type\": \"function_call_output\"", handler.RequestBodies[1]);
        Assert.Contains("Process the escalations queue.", handler.RequestBodies[1]);
    }

    private sealed class QueueHttpMessageHandler(params string[] responses) : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new(responses);

        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responses.Dequeue(), Encoding.UTF8, "application/json")
            };
        }
    }
}
