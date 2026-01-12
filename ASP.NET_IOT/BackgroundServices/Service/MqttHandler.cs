using ASP.NET_IoT.Data;
using ASP.NET_IoT.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class MqttHandler : IMqttHandler
    {
        private readonly ILogger<MqttHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private IHubContext<SensorHub> _hubContext;


        public MqttHandler(IServiceScopeFactory scopeFactory, IMemoryCache cache, IHubContext<SensorHub> hubContext, ILogger<MqttHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task HandleAsync(string topic, string payload)
        {
            //TODO: parse topic and payload

            //TODO: update cache

            //TODO: signalR send to clinet
            await _hubContext.Clients.All.SendAsync("ReceiveReading", topic, payload);

            //TODO: task insert db
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var contextHandler = scope.ServiceProvider.GetRequiredService<IContextHandler>();
                    await contextHandler.InsertPayload(payload);
                }catch(Exception ex)
                {
                    _logger.LogError(ex, "Error thorw while insert to DB");
                }
            });

        }
    }
}
