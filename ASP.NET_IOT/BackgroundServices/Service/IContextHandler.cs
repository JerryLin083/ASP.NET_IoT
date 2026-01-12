namespace ASP.NET_IoT.BackgroundServices.Service
{
    public interface IContextHandler
    {
        Task InsertPayload(string payload);
    }
}
