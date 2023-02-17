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
        public static async Task Run([ServiceBusTrigger("asb-queue", Connection = "AzureWebJobsServiceBus")] string myQueueItem, ILogger log)
        {
            var beginTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");

            var dequeuedMsg = JsonConvert.DeserializeObject<DequeuedMessage>(myQueueItem);

            var endTime = DateTime.UtcNow.AddHours(-3).ToString("HH:mm:ss");

            await SendNotificationToSlack(dequeuedMsg.Message, beginTime, endTime);
        }

        private static async Task SendNotificationToSlack(string message, string beginTime, string endTime)
        {
            using var httpClient = new HttpClient(new HttpClientHandler());

            var notification = $"O processo de LEITURA DA FILA da mensagem: [ {message} ], iníciou as {beginTime} e finalizou as {endTime}";

            var strContent = new StringContent("{'text':'" + notification + "'}");
            var slackChannelUrl = Environment.GetEnvironmentVariable("SlackNotificationChannelWebhook");

            await httpClient.PostAsync(slackChannelUrl, strContent);
        }
    }

    public class DequeuedMessage
    {
        public string Message { get; set; }
    }
}
