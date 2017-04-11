namespace DotnetSimulatedDevice
{
    using DotnetShared;
    using DotnetSharedTypes;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    class SimulatedDeviceProgram
    {
        static void Main(string[] args) { MainAsync(args).Wait(); }

        public static async Task MainAsync(string[] args)
        {
            Console.Title = "Device";
            Console.Write("Press <return> to start client device"); Console.ReadLine();

            var deviceId = Constants.DeviceID;
            var client = await GetClientAsync(deviceId: deviceId);

            var twin = await client.GetTwinAsync();
            Console.WriteLine($"Desired Software Version: {twin.Properties.Desired["SoftwareVersion"]}");

            await client.SetMethodHandlerAsync("some.method", SomeMethodAsync, null);

            while (true) {
                var message = await client.ReceiveAsync();
                Console.WriteLine($"Message {message.MessageId}");
                await client.CompleteAsync(message);
            }

            // Console.Write("Press <return> to close client device"); Console.ReadLine();
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

        static async Task<MethodResponse> SomeMethodAsync(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"Running SomeMethodAsync({ methodRequest.DataAsJson })");
            // await Task.Delay(TimeSpan.FromSeconds(2));
            var result = Encoding.UTF8.GetBytes("Hallo");

            Console.WriteLine($"Sending Response back");
            return new MethodResponse(result, 200);
        }
    }
}