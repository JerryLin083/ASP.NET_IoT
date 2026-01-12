using ASP.NET_IoT.Models.Mqtt;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public interface IChannelService
    {
        ValueTask<bool> WaitToReadAsync(CancellationToken stoppingToken);
        bool TryRead(out MqttMessage mqttMessage);
        bool TryWrite(MqttMessage mqttMessage);
    }
}
