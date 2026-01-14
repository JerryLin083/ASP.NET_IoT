using ASP.NET_IoT.Data;
using ASP.NET_IoT.Models.Mqtt;
using ASP.NET_IoT.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_IoT.BackgroundServices.Service
{
    public class ContextHandler : IContextHandler
    {
        private readonly ILogger<ContextHandler> _logger;
        private readonly IoTAppContext _context;

        public ContextHandler(IoTAppContext context, ILogger<ContextHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InsertBatch(List<MqttMessage> mqttMessages)
        {
            if (mqttMessages == null || mqttMessages.Count == 0) return;

            _logger.LogInformation($"[Batch Process] Starting batch insertion for {mqttMessages.Count} messages.");

            // 1. Extract unique DeviceIds from the incoming batch
            var incomingDeviceIds = mqttMessages
                .Where(m => m.SensorPayload != null)
                .Select(m => m.SensorPayload.DeviceId)
                .Distinct()
                .ToList();

            // 2. Fetch all existing devices in the batch in ONE query
            var existingDevices = await _context.Devices
                .Where(d => incomingDeviceIds.Contains(d.DeviceId))
                .ToDictionaryAsync(d => d.DeviceId);

            var newReadings = new List<SensorReading>();
            var devicesToUpdate = new List<Device>();

            foreach (var msg in mqttMessages)
            {
                if (msg.SensorPayload == null || msg.SensorTopic == null) continue;

                // 3. Find or Create Device in memory
                if (!existingDevices.TryGetValue(msg.SensorPayload.DeviceId, out var device))
                {
                    device = new Device
                    {
                        Id = Guid.NewGuid(),
                        DeviceId = msg.SensorPayload.DeviceId,
                        UserId = msg.SensorTopic.UserId, // Note: User reverted topic structure, might be empty
                        Area = msg.SensorTopic.Area,
                        Zone = msg.SensorTopic.Zone,
                        SensorType = msg.SensorPayload.SensorType,
                        Firmware = msg.SensorPayload.Firmware
                    };
                    _context.Devices.Add(device);
                    existingDevices[device.DeviceId] = device; // Add to dictionary to avoid duplicates in this loop
                }
                else
                {
                    // Update device metadata if changed (optional, but keep consistent with previous logic)
                    device.Area = msg.SensorTopic.Area;
                    device.Zone = msg.SensorTopic.Zone;
                    device.Firmware = msg.SensorPayload.Firmware;
                    device.SensorType = msg.SensorPayload.SensorType;
                    // If we want to capture UserId when it becomes available
                    if (string.IsNullOrEmpty(device.UserId) && !string.IsNullOrEmpty(msg.SensorTopic.UserId))
                    {
                        device.UserId = msg.SensorTopic.UserId;
                    }
                }

                // 4. Map to Reading entity
                var reading = new SensorReading
                {
                    DeviceId = device.Id,
                    Timestamp = msg.SensorPayload.Timestamp,
                    Seq = msg.SensorPayload.Seq,
                    Moisture = msg.SensorPayload.Readings?.Moisture ?? 0,
                    Temperature = msg.SensorPayload.Readings?.Temperature ?? 0,
                    Light = msg.SensorPayload.Readings?.Light ?? 0,
                    Ph = msg.SensorPayload.Readings?.Ph ?? 0,
                    Calibration = msg.SensorPayload.Readings?.Calibration,
                    Battery = msg.SensorPayload.Battery,
                    SignalStrength = msg.SensorPayload.SignalStrength,
                    Uptime = msg.SensorPayload.Uptime,
                    Device = device // Link to device entity for the current tracker context
                };
                newReadings.Add(reading);
            }

            // 5. Bulk Add and Save
            if (newReadings.Count > 0)
            {
                await _context.SensorReadings.AddRangeAsync(newReadings);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully processed batch: {newReadings.Count} readings saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch SaveChangesAsync.");
                    throw;
                }
            }
        }
    }
}
