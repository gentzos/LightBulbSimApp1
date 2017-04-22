using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace LightBulbSimApp1
{
    class MySqlDB
    {
        private static string server = "127.0.0.1";
        private static string database = "";
        private static string user = "";
        private static string pswd = "";
        private static string connectionString = "Server = " + server + ";database = " + database + ";uid = " + user + ";password = " + pswd + ";SslMode=None";

        // Register IoT Device to the Database.
        public static void registerDeviceToDB(string deviceId, string deviceType, string deviceDescription, string deviceRoom)
        {
            // Change the character encoding.
            EncodingProvider encode;
            encode = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encode);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception)
                {

                    Debug.WriteLine("There was a problem connecting to the database. Maybe is not active?");
                }

                MySqlCommand registerDevice = new MySqlCommand("INSERT INTO Devices2 (deviceId, deviceType, deviceDescription, deviceRoom) VALUES (\"" +
                    deviceId + "\", \"" + deviceType + "\", \"" + deviceDescription + "\", \"" + deviceRoom + "\")", connection);

                try
                {
                    using (MySqlDataReader reader = registerDevice.ExecuteReader())
                    {
                        reader.Read();
                        Debug.WriteLine("Device registered!");
                        connection.Close();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Device already exists!");
                    connection.Close();
                }
            }
        }

        // Send data to DB.
        public static void deviceToDB(string deviceId, double deviceValue, string dTime)
        {
            // Change the character encoding.
            EncodingProvider encode;
            encode = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encode);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception)
                {

                    Debug.WriteLine("There was a problem connecting to the database. Maybe is not active?");
                }

                MySqlCommand registerDevice = new MySqlCommand("INSERT INTO StreamData2 (deviceId, deviceValue, dTime) VALUES (\"" +
                    deviceId + "\", \"" + deviceValue + "\", \"" + dTime + "\")", connection);

                try
                {
                    using (MySqlDataReader reader = registerDevice.ExecuteReader())
                    {
                        reader.Read();
                        Debug.WriteLine("Value sent!");
                        connection.Close();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("There was a problem with sending the device value!");
                    connection.Close();
                }
            }
        }

        // Retrieve the latest value of the device from the database.
        public static double deviceLastValueFromDB(string deviceId)
        {
            // Change the character encoding.
            EncodingProvider encode;
            encode = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encode);

            double deviceValue = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception)
                {

                    Debug.WriteLine("There was a problem connecting to the database. Maybe is not active?");
                }

                MySqlCommand retrieveDevices = new MySqlCommand("SELECT deviceValue FROM streamdata2 WHERE deviceId = \"" + deviceId + "\" ORDER BY id DESC LIMIT 1", connection);
                try
                {
                    using (MySqlDataReader reader = retrieveDevices.ExecuteReader())
                    {
                        reader.Read();

                        deviceValue = (double)reader["deviceValue"];
                        Debug.WriteLine("Last known value: " + deviceValue);

                        connection.Close();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("There was a problem retrieving the device value!");
                    connection.Close();
                }
            }
            return deviceValue;
        }
    }
}

