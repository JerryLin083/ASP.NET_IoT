namespace ASP.NET_IoT.Models.Mqtt
{
    public class SensorTopic
    {
        public string UserId { get; init; } = string.Empty;
        public string Area {  get; init; } = string.Empty;
        public string Zone { get; init; } = string.Empty;

        public bool IsValid { get; init; }
        
        public static SensorTopic Parse(string topic)
        {
            if(string.IsNullOrEmpty(topic)) return new SensorTopic() { IsValid = false};

            var parts = topic.Split("/", StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length < 3 || parts[0] != "sensors") return new SensorTopic() { IsValid = false};

            return new SensorTopic
            {
                Area = parts[1],
                Zone = parts[2],
                IsValid = true
            };
        }
    }
}
