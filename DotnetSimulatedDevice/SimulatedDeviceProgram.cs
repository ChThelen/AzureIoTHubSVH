namespace DotnetSimulatedDevice
{
    using DotnetSharedTypes;
    using Microsoft.Azure.Devices.Client;
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
            var client = DeviceClient.Create(
                hostname: cred.Hostname,
                authenticationMethod: new DeviceAuthenticationWithToken(
                    deviceId: cred.DeviceId,
                    token: cred.SharedAccessSignature));

            var twin = await client.GetTwinAsync();
            Console.WriteLine($"{twin.DeviceId}");
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
