namespace DotnetSimulatedDevice
{
    using DotnetSharedTypes;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class SimulatedDeviceProgram
    {
        static void Main(string[] args) { MainAsync(args).Wait(); }

        public static async Task MainAsync(string[] args)
        {
            Console.Title = "Device";

            var cred = await LogonToBackendAsync();
            Console.WriteLine($"I am {cred.DeviceId}");

        }

        public static async Task<DeviceCredentials> LogonToBackendAsync()
        {
            // Make an authenticated REST call to the PushMessage-Backend, and retrieve DeviceID and SAS for IoT Hub.
            // in our simulated environment, get data from local file

            var credPath = @"..\..\..\deviceCred.json";

            return JsonConvert.DeserializeObject<DeviceCredentials>(File.ReadAllText(credPath));
        }
    }
}
