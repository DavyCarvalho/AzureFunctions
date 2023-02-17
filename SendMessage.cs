using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsWithServiceBus
{
    public static class SendMessage
    {
        [FunctionName("SendMessage")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            var beginTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");
            
            var body = await GetParsedBody(req);

            await SendMessageToAzureQueue(body);

            var endTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");

            await SendNotificationToSlack(body.Message, beginTime, endTime);

            log.LogInformation($"SendMessage processed.");
        }

        private static async Task<Body> GetParsedBody(HttpRequest req)
        {
            using var reader = new StreamReader(req.Body, Encoding.UTF8);
            
            var bodyString = await reader.ReadToEndAsync();

            return JsonConvert.DeserializeObject<Body>(bodyString);
        }

        private static async Task SendNotificationToSlack(string message, string beginTime, string endTime)
        {
            using var httpClient = new HttpClient(new HttpClientHandler());

            var notification = $"O processo de REGISTRO NA FILA da mensagem: [ {message} ], iníciou as {beginTime} e finalizou as {endTime}";

            var strContent = new StringContent("{'text':'" + notification + "'}");
            var slackChannelUrl = Environment.GetEnvironmentVariable("SlackNotificationChannelWebhook");

            await httpClient.PostAsync(slackChannelUrl, strContent);
        }

        private static async Task SendMessageToAzureQueue(Body body)
        {
            var queueClient = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsServiceBus"), "asb-queue");

            byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
            var message = new Message(messageBytes);
            await queueClient.SendAsync(message);
        }
    }

    public class Body
    {
        public string Message { get; set; }
    }
}
