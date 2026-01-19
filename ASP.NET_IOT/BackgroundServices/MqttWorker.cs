using ASP.NET_IoT.BackgroundServices.Service;
using MQTTnet;
using System.Text;

namespace ASP.NET_IoT.BackgroundServices
{
    public class MqttWorker: BackgroundService
    {
        private readonly ILogger<MqttWorker> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly IConfiguration _configuration;
        private readonly MqttClientOptions _options;
        private readonly IMqttHandler _mqttHandler;

        public MqttWorker(ILogger<MqttWorker> logger, IConfiguration configuration, IMqttHandler mqttHandler)
        {
            _logger = logger;
            _configuration = configuration;
            _mqttHandler = mqttHandler;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(_configuration["MQTT:Broker"])
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                .WithClientId($"AspNetCore_{Guid.NewGuid()}")
                .Build();


            SetupEvents();
        }

        private  void SetupEvents() {
            // 1. handle receive message
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                await _mqttHandler.HandleAsync(topic, payload);
            };

            // 2. auto subscribe topic
            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Successfully connect to borker");

                var topicFiler = new MqttTopicFilterBuilder().WithTopic(_configuration["MQTT:Topic"]).Build();

                await _mqttClient.SubscribeAsync(topicFiler);
                _logger.LogInformation($"Subscribe: {_configuration["MQTT:Topic"]}");
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
