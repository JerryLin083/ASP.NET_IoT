using Microsoft.EntityFrameworkCore;
using ASP.NET_IoT.Models.Entities;

namespace ASP.NET_IoT.Data
{
    public class IoTAppContext: DbContext
    {
        public IoTAppContext(DbContextOptions<IoTAppContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
    }
}
