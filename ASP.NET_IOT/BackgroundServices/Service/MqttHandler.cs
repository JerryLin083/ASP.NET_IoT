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
            //parse topic and payload
            var sensorTopic = ParseTopic(topic);
            var sensorPayload = ParsePayload(payload);

            if(!sensorTopic.IsValid || sensorPayload == null)
            {
                _logger.LogWarning("Failed to parse topic/payload");
                return;
            }

            MqttMessage mqttMessage = new MqttMessage(topic, payload, sensorTopic, sensorPayload);

            // 1. Update In-Memory Cache with expiration to prevent memory bloat
            var cacheKey = $"latest_{sensorTopic.Area}_{sensorTopic.Zone}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)) // Hard limit to refresh metadata
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, mqttMessage.SensorPayload, cacheOptions);

            // 2. Notify clients via SignalR (Real-time dashboard)
            await _hubContext.Clients.All.SendAsync("ReceiveReading", mqttMessage.RowTopic, mqttMessage.RowPayload);

            // 3. Queue for Batch Database Insertion
            if (!_channelService.TryWrite(mqttMessage))
            {
                _logger.LogWarning("Channel full: Mqtt message dropped from DB queue. Topic: {Topic}", topic);
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
