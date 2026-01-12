using ASP.NET_IoT.Data;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class ContextHandler: IContextHandler
    {
        private readonly ILogger<ContextHandler> _logger;
        private readonly IoTAppContext _context;

        public ContextHandler(IoTAppContext context, ILogger<ContextHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InsertPayload(string payload)
        {
            //TODO: insert payload to db
            _logger.LogInformation($"payload: {payload}, inserted");
        }

    }
}
