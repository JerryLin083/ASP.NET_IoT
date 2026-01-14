using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.NET_IoT.Models.Entities
{
    public class SensorReading
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public Guid DeviceId { get; set; }

        public long Timestamp { get; set; }

        public int Seq { get; set; }

        public double Moisture { get; set; }

        public double Temperature { get; set; }

        public double Light { get; set; }

        public double Ph { get; set; }

        [MaxLength(200)]
        public string? Calibration { get; set; }

        public int Battery { get; set; }

        public int SignalStrength { get; set; }

        public long Uptime { get; set; }

        // Navigation property
        [ForeignKey("DeviceId")]
        public Device Device { get; set; } = null!;
    }
}
