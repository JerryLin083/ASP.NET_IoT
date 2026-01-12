using ASP.NET_IoT.Models.Mqtt;
using System.Threading.Channels;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class ChannelService: IChannelService
    {
        private readonly Channel<MqttMessage> _channel;

        public ChannelService(int capacity = 1000) {
            _channel = Channel.CreateBounded<MqttMessage>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public  ValueTask<bool> WaitToReadAsync(CancellationToken stoppingToken)
        {
            return _channel.Reader.WaitToReadAsync(stoppingToken); 
        }

        public bool TryRead(out MqttMessage mqttMessage)
        {
            return _channel.Reader.TryRead(out mqttMessage);
        }

        public bool TryWrite(MqttMessage message) { 
            return _channel.Writer.TryWrite(message);
        }
    }
}
