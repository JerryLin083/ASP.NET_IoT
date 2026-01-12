using ASP.NET_IoT.Data;
using ASP.NET_IoT.Models.Mqtt;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class ContextHandler : IContextHandler
    {
        private readonly ILogger<ContextHandler> _logger;
        private readonly IoTAppContext _context;

        public ContextHandler(IoTAppContext context, ILogger<ContextHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InsertBatch(List<MqttMessage> mqttMessages)
        {
            // TODO: actual database insertion using EF Core
            _logger.LogInformation($"[Batch Process] Received {mqttMessages.Count} messages for processing.");
            
            foreach (var mqttMessage in mqttMessages)
            {
                _logger.LogDebug($"Processing message: Topic={mqttMessage.RowTopic}, Payload={mqttMessage.RowPayload}");
            }

            await Task.CompletedTask;
        }
    }
}
