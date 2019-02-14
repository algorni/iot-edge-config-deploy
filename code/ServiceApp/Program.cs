using Microsoft.Azure.Devices;
using System;
using System.IO;

namespace ServiceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine(" Push Configuration to Configuration Module app sample ");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine();

            ///OF COURSE THESE DETAILS SHOULD COME FROM SOME KIND OF CONFIGURATION. NOT HARDCODED THIS IS JUST A SAMPLE

            //that's teh IoT Hub Connection String to which teh device is connected to.
            var iotHubConnection = "<insert here your iot hub service connection string>";

            //that's the device ID where you want to deploy your configuration
            var deviceId = "<enter the device id>";

            //this is the module name that handle the configuration 
            var moduleId = "ConfigurationChangeHandlerModule";

            var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnection);

            var method = new CloudToDeviceMethod("SetConfig");

            //this is an example of your configuration that want to deploy to the module
            var config = new
                {
                    Option1 = "value",
                    Option2 = "value", 
                    Option3 = "value"
                };
            
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(config);

            method.SetPayloadJson(jsonString);
            
            var result = serviceClient.InvokeDeviceMethodAsync(deviceId, moduleId, method).Result;

            Console.WriteLine($"Result: {result.Status}");
        }
    }
}
