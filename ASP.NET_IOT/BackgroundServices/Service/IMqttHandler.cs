namespace ASP.NET_IoT.BackgroundServices.Service
{
    public interface IMqttHandler
    {
        Task HandleAsync(string topic, string payload);
    }
}
