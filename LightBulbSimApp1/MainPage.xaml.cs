using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightBulbSimApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Initialize the IoT Device as Thermostat.
        private IoTDevice iotDevice = new IoTDevice()
        {
            deviceId = "lb01",
            deviceType = "lightbulb",
            deviceDescription = "An IoT Light Bulb",
            deviceValue = 0,
            deviceRoom = "Kitchen",
            deviceHubAccessKey = ""
        };

        public MainPage()
        {
            this.InitializeComponent();

            // Display the IoT device values in GUI.
            textBlockDeviceIdChange.Text = iotDevice.deviceId;
            textBlockDeviceTypeChange.Text = iotDevice.deviceType;
            textBlockDeviceDescriptionChange.Text = iotDevice.deviceDescription;
            textBlockDeviceRoomChange.Text = iotDevice.deviceRoom;

            // Register device to Azure IoT Hub and local SQL Database.
            registerDevice();

            // Retrieve latest known value.
            iotDevice.deviceValue = MySqlDB.deviceLastValueFromDB(iotDevice.deviceId);
            if (iotDevice.deviceValue == 1)
                toggleButton.IsChecked = true;
            else
                toggleButton.IsChecked = false;

            // Send device value to SQL DB every second.
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
            {
                //do some work \ dispatch to UI thread as needed
                deviceToDB();
            }, TimeSpan.FromSeconds(1));

            //// Send messages from device to cloud.
            //deviceToCloud(iotDevice.deviceHubAccessKey);
        }

        // Change the state of the light when toggle button is checked
        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            textBlockDeviceValueChange.Text = "On";
            iotDevice.deviceValue = 1;
        }

        // Change the state of the light when toggle button is unchecked
        private void toggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            textBlockDeviceValueChange.Text = "Off";
            iotDevice.deviceValue = 0;
        }

        // Register device to Azure IoT Hub and local SQL Database.
        private async void registerDevice()
        {
            // Register the device to the cloud and receive the access key.
            iotDevice.deviceHubAccessKey = await AzureIoTHub.AddDeviceAsync(iotDevice.deviceId);

            // Register the device to the database.
            MySqlDB.registerDeviceToDB(iotDevice.deviceId, iotDevice.deviceType, iotDevice.deviceDescription, iotDevice.deviceRoom);

            cloudToDevice(iotDevice.deviceHubAccessKey);
        }

        // .
        private void deviceToDB()
        {
            // Get current date and time.
            DateTime dTime = DateTime.Now;

            // Send device value to the database.
            MySqlDB.deviceToDB(iotDevice.deviceId, iotDevice.deviceValue, dTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

        // Device to Cloud Communication.
        //private async void deviceToCloud(string deviceSharedAccessKey)
        //{
        //    // Get current date and time.
        //    DateTime dt;

        //    // 
        //    var registerTelemetryMessage = new AzureIoTHub.Telemetry
        //    {
        //        deviceStatus = 0,
        //        deviceId = iotDevice.deviceId,
        //        deviceType = iotDevice.deviceType,
        //        deviceDescription = iotDevice.deviceDescription,
        //        deviceValue = iotDevice.deviceValue,
        //        deviceRoom = iotDevice.deviceRoom,
        //    };
        //    await AzureIoTHub.SendDeviceToCloudMessageAsync(registerTelemetryMessage, iotDevice.deviceId, deviceSharedAccessKey);

        //    while (true)
        //    {
        //        // Get current date and time
        //        dt = DateTime.Now;
        //        // Get seconds, convert them to string and pass them in the GUI
        //        textBlockSecondsValue.Text = System.Convert.ToString(dt.Second);

        //        // If a minute is passed send date, time 
        //        // and temperature value to the cloud
        //        if (dt.Second == 00)
        //        {
        //            var telemetryMessage = new AzureIoTHub.Telemetry
        //            {
        //                deviceStatus = 1,
        //                deviceId = iotDevice.deviceId,
        //                deviceType = "",
        //                deviceDescription = "",
        //                deviceValue = iotDevice.deviceValue,
        //                deviceRoom = "",
        //                dTime = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
        //            };
        //            await AzureIoTHub.SendDeviceToCloudMessageAsync(telemetryMessage, iotDevice.deviceId, deviceSharedAccessKey);
        //        }
        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //    }
        //}

        // Listen for messages from the cloud
        // Set lightbulb value according to the cloud message
        private async void cloudToDevice(string deviceSharedAccessKey)
        {
            while (true)
            {
                string receivedMessage = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync(iotDevice.deviceId, deviceSharedAccessKey);
                iotDevice.deviceValue = Convert.ToDouble(receivedMessage);
                if (iotDevice.deviceValue == 1)
                    toggleButton.IsChecked = true;
                else
                    toggleButton.IsChecked = false;
            }
        }
    }
}
