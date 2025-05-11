using Newtonsoft.Json;
using System.Configuration;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using System;
using xPlannerCommon.Extensions;
using Microsoft.Owin.Security.Provider;

namespace xPlannerAPI.Services
{
    public class WebjobRepository<T>
    {
        private readonly string _connectionString;

        public WebjobRepository()
        {
            this._connectionString = AudaxWareConfiguration.GetWebJobsStorage();
        }
        public async Task<bool> SendMessage(string queueName, T item)
        {
            var queueClient = new QueueClient(this._connectionString, queueName,
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

            var queueMessage = JsonConvert.SerializeObject(item);
            await queueClient.SendMessageAsync(queueMessage);
            return true;
        }
    }
}