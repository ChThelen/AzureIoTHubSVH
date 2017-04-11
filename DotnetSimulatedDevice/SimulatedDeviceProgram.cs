namespace DotnetSimulatedDevice
{
    using DotnetSharedTypes;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Threading.Tasks;

    class SimulatedDeviceProgram
    {
        static void Main(string[] args) { MainAsync(args).Wait(); }

        public static async Task MainAsync(string[] args)
        {
            Console.Title = "Device";
            Console.Write("Press <return"); Console.ReadLine();

            var client = await GetClientAsync(deviceId: "id:device:000001");
            var twin = await client.GetTwinAsync();
            Console.WriteLine($"{twin.DeviceId}");
        }

        public static async Task<DeviceClient> GetClientAsync(string deviceId)
        {
            var creds = await MyExtensions.GetDeviceCredentialsAsync(deviceId);
            return DeviceClient.Create(
                hostname: creds.Hostname,
                authenticationMethod: new DeviceAuthenticationWithToken(
                    deviceId: deviceId,
                    token: creds.SharedAccessSignature), 
                transportType: TransportType.Mqtt_WebSocket_Only);
        }
    }
}