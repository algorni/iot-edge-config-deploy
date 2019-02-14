namespace ConfigurationReceiverModule
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


            //Well this module is not doing nothing specially....    just run and look at the config...

            string configJson = File.ReadAllText(_configFilePath);

            Console.WriteLine("Configuration read:");
            Console.WriteLine(configJson);
            
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

     
    }
}
