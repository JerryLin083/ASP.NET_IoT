using ASP.NET_IoT.Hubs;
using ASP.NET_IoT.Models.Mqtt;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class MqttHandler : IMqttHandler
    {
        private readonly ILogger<MqttHandler> _logger;
        private readonly IMemoryCache _cache;
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly IChannelService _channelService;

        public MqttHandler(IMemoryCache cache, IHubContext<SensorHub> hubContext, ILogger<MqttHandler> logger, IChannelService channelService)
        {
            _cache = cache;
            _hubContext = hubContext;
            _logger = logger;
            _channelService = channelService;
        }

        public async Task HandleAsync(string topic, string payload)
        {
            //TODO: parse topic and payload
            var sensorTopic = ParseTopic(topic);
            var sensorPayload = ParsePayload(payload);

            if(!sensorTopic.IsValid || sensorPayload == null)
            {
                _logger.LogWarning("Failed to parse topic/payload");
                return;
            }

            MqttMessage mqttMessage = new MqttMessage(topic, payload, sensorTopic, sensorPayload);

            //TODO: update cache
            _cache.Set($"latest_{sensorTopic.Area}_${sensorTopic.Zone}", mqttMessage.SensorPayload);

            //TODO: signalR send to clinet
            await _hubContext.Clients.All.SendAsync("ReceiveReading", topic, payload);

            //TODO: task insert db
            try
            {
                //send mqtt message to channel
                if (!_channelService.TryWrite(mqttMessage))
                {
                    _logger.LogWarning($"Failed to send mqtt message to channel");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error thorw while insert to DB");
            }
        }

        private static SensorTopic ParseTopic(string topic)
        {
            return SensorTopic.Parse(topic);
        }

        private static SensorPayload? ParsePayload(string payload)
        {
            try
            {
                var sensorPayload = JsonSerializer.Deserialize<SensorPayload>(payload.TrimEnd('\0')); // trim useless buffer
                return sensorPayload;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
