using MQTTnet;
using System.Text;

namespace ASP.NET_IoT.BackgroundServices
{
    public class MqttWorker: BackgroundService
    {
        private readonly ILogger<MqttWorker> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;

        public MqttWorker(ILogger<MqttWorker> logger)
        {
            _logger = logger;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com")
                .WithClientId($"AspNetCore_{Guid.NewGuid()}")
                .Build();

            SetupEvents();
        }

        private void SetupEvents() {
            // 1. handle receive message
            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                _logger.LogInformation($"Receive message! Topic: {topic}, Payload: {payload}");

                //TODO: update cache and signalR broadcast

                return Task.CompletedTask;
            };

            // 2. auto subscribe topic
            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Successfully connect to borker");

                var topicFiler = new MqttTopicFilterBuilder().WithTopic("sensors/#").Build();

                await _mqttClient.SubscribeAsync(topicFiler);
                _logger.LogInformation("Subscribe: sensors/#");
            };

            // 3.handle disconnect
            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("MQTT disconnected, try reconnect in 5 sec");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _mqttClient.ConnectAsync(_options);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Reconnected Failed: {ex.Message}");
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MQTT init failed: {ex.Message}");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            
        }
    }
}
