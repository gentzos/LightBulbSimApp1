using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Collections.Generic;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Diagnostics;


namespace LightBulbSimApp1
{
    class AzureIoTHub
    {
        //
        // Note: this connection string is specific to the device "thermostat01". To configure other devices,
        // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
        //
        //
        // Azure IoT Hub Host Name
        const string hubHostName = "";

        // Info for connection to the Azure IoT Hub
        const string hubSharedAccessKeyName = "";
        const string hubSharedAccessKey = "";
        // Conncetion string for hub communication
        const string hubConnectionString = "HostName=" + hubHostName + ";SharedAccessKeyName=" + hubSharedAccessKeyName +
            ";SharedAccessKey=" + hubSharedAccessKey;

        //
        // To monitor messages sent to device "thermostat01" use iothub-explorer as follows:
        //   

        // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

        public static async Task<string> AddDeviceAsync(string deviceId)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(hubConnectionString);
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            //Debug.WriteLine(device.Authentication.SymmetricKey.PrimaryKey);
            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        public static async Task SendDeviceToCloudMessageAsync(Telemetry telemetry, string deviceId, string deviceSharedAccessKey)
        {
            // Conncetion string for device communication
            string deviceConnectionString = "HostName=" + hubHostName + ";DeviceId=" + deviceId +
                ";SharedAccessKey=" + deviceSharedAccessKey;

            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp);

            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(
                    Newtonsoft.Json.JsonConvert.
                      SerializeObject(telemetry)));

            await deviceClient.SendEventAsync(message);
        }

        public static async Task<string> ReceiveCloudToDeviceMessageAsync(string deviceId, string deviceSharedAccessKey)
        {
            // Conncetion string for device communication
            string deviceConnectionString = "HostName=" + hubHostName + ";DeviceId=" + deviceId +
                ";SharedAccessKey=" + deviceSharedAccessKey;

            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp);

            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);
                    return messageData;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public class Telemetry
        {
            public int deviceStatus { get; set; }
            public string deviceId { get; set; }
            public string deviceType { get; set; }
            public string deviceDescription { get; set; }
            public int deviceValue { get; set; }
            public string deviceRoom { get; set; }
            public string dTime { get; set; }
        }
    }
}
