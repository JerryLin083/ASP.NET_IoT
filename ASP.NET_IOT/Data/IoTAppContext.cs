using Microsoft.EntityFrameworkCore;

namespace ASP.NET_IoT.Data
{
    public class IoTAppContext: DbContext
    {
        IoTAppContext(DbContextOptions<IoTAppContext> options) : base(options) { }
    }
}
