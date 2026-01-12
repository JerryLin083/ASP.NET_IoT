using ASP.NET_IoT.BackgroundServices.Service;
using ASP.NET_IoT.Models.Mqtt;

namespace ASP.NET_IoT.BackgroundServices
{
    public class MqttDbBatchService : BackgroundService
    {
        private readonly IChannelService _channelService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttDbBatchService> _logger;
        private readonly List<MqttMessage> _mqttMessages = [];

        public MqttDbBatchService(IChannelService channelService, IServiceScopeFactory scopeFactory, ILogger<MqttDbBatchService> logger, int capacity = 100)
        {
            _channelService = channelService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Db channel worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Wait until at least one item is available or the service is stopping
                    if (!await _channelService.WaitToReadAsync(stoppingToken))
                    {
                        break;
                    }

                    // 2. Collect items in a batch (up to 10 items or 5 seconds timeout)
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));

                    while (_mqttMessages.Count < 10)
                    {
                        try
                        {
                            if (_channelService.TryRead(out var mqttMessage))
                            {
                                _mqttMessages.Add(mqttMessage);
                            }
                            else
                            {
                                // No more items right now, wait for a bit or until timeout
                                await Task.Delay(100, cts.Token);
                                if (!await _channelService.WaitToReadAsync(cts.Token)) break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break; // Stop collecting if 5s timeout or service stopping
                        }
                    }

                    // 3. Process the batch
                    if (_mqttMessages.Count > 0)
                    {
                        _logger.LogInformation($"Processing batch of {_mqttMessages.Count} messages.");
                        using var scope = _scopeFactory.CreateScope();
                        try
                        {
                            var context = scope.ServiceProvider.GetRequiredService<IContextHandler>();
                            await context.InsertBatch([.._mqttMessages]);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to insert batch into database.");
                        }
                        finally
                        {
                            _mqttMessages.Clear();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MqttDbBatchService execution loop.");
                    await Task.Delay(1000, stoppingToken); // Wait a bit before retrying on general error
                }
            }

            _logger.LogInformation("Db channel worker stopped.");
        }
    }
}
