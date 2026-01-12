using System.Text.Json.Serialization;

namespace ASP.NET_IoT.Models.Mqtt
{
    public class SensorPayload
    {
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("seq")]
        public int Seq { get; set; }

        [JsonPropertyName("sensor_type")]
        public string SensorType { get; set; } = string.Empty;

        [JsonPropertyName("readings")]
        public SensorReadings? Readings { get; set; }

        [JsonPropertyName("battery")]
        public int Battery { get; set; }

        [JsonPropertyName("firmware")]
        public string Firmware { get; set; } = string.Empty;

        [JsonPropertyName("signal_strength")]
        public int SignalStrength { get; set; }

        [JsonPropertyName("uptime")]
        public long Uptime { get; set; }
    }

    public class SensorReadings
    {
        [JsonPropertyName("moisture")]
        public double Moisture { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("light")]
        public double Light { get; set; }

        [JsonPropertyName("ph")]
        public double Ph { get; set; }

        [JsonPropertyName("calibration")]
        public string Calibration { get; set; } = string.Empty;
    }
}
