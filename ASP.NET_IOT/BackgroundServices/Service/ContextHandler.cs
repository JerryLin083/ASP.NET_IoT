using ASP.NET_IoT.Data;
using ASP.NET_IoT.Models.Mqtt;
using System.Text.Json;

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

        public async Task InsertPayload(MqttMessage mqttMessage)
        {
            //TODO: insert payload to db
            _logger.LogInformation($"payload: {mqttMessage.RowPayload}, inserted");
        }
    }
}
