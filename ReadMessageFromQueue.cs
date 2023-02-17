using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

namespace ReadMessageFromQueue
{
    public class ReadMessageFromQueue
    {
        [FunctionName("ReadMessageFromQueue")]
        public static async Task Run([ServiceBusTrigger("asb-queue", Connection = "AzureWebJobsServiceBus")]string myQueueItem, ILogger log)
        {
            var beginTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");

            var dequeuedMsg = JsonConvert.DeserializeObject<DequeuedMessage>(myQueueItem);

            await SendMessagesToSlack(dequeuedMsg.Message, beginTime);
        }

        private static async Task SendMessagesToSlack(string message, string beginTime)
        {
            using var httpClient = new HttpClient(new HttpClientHandler());

            await SendMessageFromQueue(message, httpClient);

            await SendLogMessage(message, beginTime, httpClient);
        }

        private static async Task SendMessageFromQueue(string message, HttpClient httpClient)
        {
            var strContent = new StringContent("{'text':'" + message + "'}");
            var slackMsgChannelUrl = Environment.GetEnvironmentVariable("SlackMessageChannelWebhook");

            await httpClient.PostAsync(slackMsgChannelUrl, strContent);
        }

        private static async Task SendLogMessage(string message, string beginTime, HttpClient httpClient)
        {
            var endTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");
            var logNotification = $"O processo de LEITURA DA FILA da mensagem: [ {message} ], iníciou as {beginTime} e finalizou as {endTime}";

            var strContent = new StringContent("{'text':'" + logNotification + "'}");
            var slackLogChannelUrl = Environment.GetEnvironmentVariable("SlackProcessLoggingChannelWebhook");

            await httpClient.PostAsync(slackLogChannelUrl, strContent);
        }
    }

    public class DequeuedMessage
    {
        public string Message { get; set; }
    }
}
