namespace DotnetSimulatedDevice
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using DotnetShared;

    class Device
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
                if (message != null)
                {
                    Console.WriteLine($"Message {message.MessageId}");
                    await client.CompleteAsync(message);
                }
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
                transportType: TransportType.Mqtt_Tcp_Only);
        }

        static async Task<MethodResponse> SomeMethodAsync(MethodRequest methodRequest, object userContext)
        {
            // await Task.Delay(TimeSpan.FromSeconds(2));
            Console.WriteLine($"Running SomeMethodAsync({ methodRequest.DataAsJson })");

            var result = Encoding.UTF8.GetBytes(@"{ 
                'reply': 'Hallo' 
            }");

            // MethodResponse has a bad API. It accepts arbitraty byte[]s, but internally checks for JSON 
            // https://github.com/Azure/azure-iot-sdk-csharp/blob/master/device/Microsoft.Azure.Devices.Client/MethodResponse.cs#L48
            return new MethodResponse(result, 200);
        }
    }
}