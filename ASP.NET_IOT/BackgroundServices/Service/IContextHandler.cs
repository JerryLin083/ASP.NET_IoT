using ASP.NET_IoT.Models.Mqtt;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public interface IContextHandler
    {
        Task InsertBatch(List<MqttMessage> mqttMessages);
    }
}
