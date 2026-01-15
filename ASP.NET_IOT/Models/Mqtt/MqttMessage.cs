namespace ASP.NET_IoT.Models.Mqtt
{
    public class MqttMessage
    {
        public string RowTopic { get; set; }    
        public string RowPayload {  get; set; } 

        // after parse
        public SensorTopic SensorTopic { get; set; }
        public SensorPayload SensorPayload{ get; set; }

        public MqttMessage(string rowTopic, string rowPayload, SensorTopic sensorTopic, SensorPayload sensorPayload)
        {
            RowTopic = rowTopic;
            RowPayload = rowPayload.TrimEnd('\0');
            SensorTopic = sensorTopic;
            SensorPayload = sensorPayload;
        }
    }
}
