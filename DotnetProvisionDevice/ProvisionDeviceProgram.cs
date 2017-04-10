namespace DotnetProvisionDevice
{
    using DotnetSharedTypes;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args) { MainAsync(args).Wait(); }

        public static async Task MainAsync(string[] args)
        {
            Console.Title = "Device Provisioning";

            // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-security
            var connectionString = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");
            var deviceId = "id:device:000001";

            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            // https://docs.microsoft.com/de-de/azure/iot-hub/iot-hub-csharp-csharp-getstarted
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(id: deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId: deviceId);
            }


            // Console.WriteLine($"{device.GenerationId} {device.Id} {device.Authentication.SymmetricKey.PrimaryKey}");

            // https://github.com/Azure/azure-content-nlnl/blob/master/articles/iot-hub/iot-hub-guidance.md#customauth

            var cred = new DeviceCredentials
            {
                DeviceId = device.Id,
                PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey
            };

            var credPath = @"..\..\..\deviceCred.json";
            File.WriteAllText(credPath, JsonConvert.SerializeObject(cred));
            Console.WriteLine($"Wrote device credential to {new FileInfo(credPath).FullName}");



        }
    }
}
