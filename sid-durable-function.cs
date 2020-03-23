using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Refit;

namespace Sid.DurableFunction
{
    public static class sid_durable_function
    {
        [FunctionName("sid_durable_function")]
        public static async Task<PostmanEchoPostResponse> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var tags = context.GetInput<List<string>>();

            return await context.CallActivityAsync<PostmanEchoPostResponse>("sid_durable_function_SendRequest", tags);
        }

        [FunctionName("sid_durable_function_SendRequest")]
        public static async Task<PostmanEchoPostResponse> SendRequest(
            [ActivityTrigger] List<string> tags,
            ILogger log)
        {
            var postmanEcho = RestService.For<IPostmanEchoApi>("https://postman-echo.com");
            var response = await postmanEcho.Post(new TagRequest(tags.ToArray()));
            log.LogInformation($"Received response from Postman echo service: {response.Json.Objects.FirstOrDefault()}");
            
            return response;
        }

        [FunctionName("sid_durable_function_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            var tags = await req.Content.ReadAsAsync<List<string>>();

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("sid_durable_function", tags);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}