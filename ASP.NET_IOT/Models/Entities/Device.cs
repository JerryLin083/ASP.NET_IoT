using System.ComponentModel.DataAnnotations;

namespace ASP.NET_IoT.Models.Entities
{
    public class Device
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; } // Soft-link to Identity User Id

        [MaxLength(100)]
        public string? Area { get; set; }

        [MaxLength(100)]
        public string? Zone { get; set; }

        [MaxLength(100)]
        public string? SensorType { get; set; }

        [MaxLength(50)]
        public string? Firmware { get; set; }

        // Navigation property
        public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
    }
}
