using Microsoft.EntityFrameworkCore;

namespace ASP.NET_IoT.Data
{
    public class IoTAppContext: DbContext
    {
        public IoTAppContext(DbContextOptions<IoTAppContext> options) : base(options) { }
    }
}
