namespace DotnetProvisionDevice
{
    using DotnetShared;
    using DotnetSharedTypes;
    using Microsoft.Azure.Devices;
    using Microsoft.Owin.Hosting;
    using Owin;
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    class ProvisionDeviceProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Device Provisioning Service";
            using (WebApp.Start<Startup>(Constants.ProvisioningServer))
            {
                Console.WriteLine("Press <return> to close");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            appBuilder.UseWebApi(config);
        }
    }

    public class DeviceProvisionerController : ApiController
    {
        readonly string connectionString;
        readonly string hubHostName;
        readonly RegistryManager registryManager;

        public DeviceProvisionerController()
        {
            connectionString = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");
            hubHostName = connectionString.ParseConnectionString()["HostName"];
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }

        [HttpGet]
        [Route(Constants.ProvisioningServerPath + "/{deviceId}")]
        public async Task<DeviceCredentials> GetTokenAsync(string deviceId)
        {
            // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-security
            // https://docs.microsoft.com/de-de/azure/iot-hub/iot-hub-csharp-csharp-getstarted
            // https://github.com/Azure/azure-content-nlnl/blob/master/articles/iot-hub/iot-hub-guidance.md#customauth

            var device = await GetOrCreateDeviceAync(deviceId: deviceId);

            var sas = MyExtensions.GetSASToken(
                hubHostname: hubHostName,
                deviceId: device.Id,
                deviceKey: Convert.FromBase64String(device.Authentication.SymmetricKey.PrimaryKey),
                duration: TimeSpan.FromHours(1));

            Console.WriteLine($"Created token for {deviceId}");

            return new DeviceCredentials { Hostname = hubHostName, DeviceId = deviceId, SharedAccessSignature = sas };
        }

        private async Task<Device> GetOrCreateDeviceAync(string deviceId)
        {
            var device = await registryManager.GetDeviceAsync(deviceId: deviceId);
            if (device != null)
            {
                var twin = await registryManager.GetTwinAsync(deviceId: deviceId);

                return device;
            }
            device = await registryManager.AddDeviceAsync(new Device(id: deviceId));

            await SetTwinData(deviceId);

            return device;
        }

        private async Task SetTwinData(string deviceId)
        {
            var twin = await registryManager.GetTwinAsync(deviceId: deviceId);
            twin.Tags["location"] = new { region = "Germany" }; 
            twin.Properties.Desired["SoftwareVersion"] = "1.2.3.4";
            await registryManager.UpdateTwinAsync(deviceId: deviceId, twinPatch: twin, etag: twin.ETag);
        }
    }
}