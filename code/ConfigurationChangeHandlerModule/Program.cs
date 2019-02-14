namespace ConfigurationModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration;

    class Program
    {
        static IConfigurationRoot _configuration;

        static string _configFilePath;

        static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" Configuration Module");
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();


            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();

            _configuration = builder.Build();

            Console.WriteLine("Configuration built from Environment");

            _configFilePath = _configuration.GetValue<string>("CM_CONFIG_FILE_PATH");

            Console.WriteLine($"CM_CONFIG_FILE_PATH = {_configFilePath}");
            
            Init().Wait();

            Console.WriteLine("Waiting for Configuration from IoT Hub");

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();

            Console.WriteLine("IoT Hub module client initialized.");

            //Register for the Direct Method call
            await ioTHubModuleClient.SetMethodHandlerAsync("SetConfig", updateConfigMethodCallback, ioTHubModuleClient);

            Console.WriteLine("UpdateConfig Direct Method callback registered.");
        }

        static Task<MethodResponse> updateConfigMethodCallback(MethodRequest methodRequest, object userContext)
        {  
            Console.WriteLine($"Update Configuration callback");

            MethodResponse response = null;
            
            string configurationContent = Encoding.UTF8.GetString(methodRequest.Data);

            try
            {
                //ok write the file...
                File.WriteAllText(_configFilePath, configurationContent);

                var message = $"Configuration updated in the mounted folder.\n\n{configurationContent}";

                Console.WriteLine(message);

                var responseContent = new { Status = message };

                byte[] messageBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(responseContent));

                response = new MethodResponse(messageBytes, 200);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error while storing the configuration file contnet into path {_configFilePath}\n\n{ex.ToString()}";
                
                Console.WriteLine(errorMessage);

                var responseContent = new { Status = errorMessage };

                byte[] messageBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(responseContent));

                response = new MethodResponse(messageBytes, 500);
            }

            return Task.FromResult(response);
        }
    }
}
