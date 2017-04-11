namespace DotnetBackend
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using DotnetShared;

    class Backend
    {
        static void Main(string[] args) { MainAsync(args).Wait(); }

        public static async Task MainAsync(string[] args)
        {
            Console.Title = "Backend";
            var connectionString = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            var deviceId = Constants.DeviceID;

            Console.Write("Press <return> to invoke 'some.method'");
            Console.ReadLine();

            var cloudToDeviceMethod = new CloudToDeviceMethod(
                    methodName: "some.method"
                    //responseTimeout: TimeSpan.FromSeconds(5),
                    //connectionTimeout: TimeSpan.FromMinutes(1));
                    );
            cloudToDeviceMethod.SetPayloadJson(@"{ 
                fwPackageUri : 'https://someurl' 
            }");

            // var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            // var result = await serviceClient.InvokeDeviceMethodAsync(deviceId: deviceId, cloudToDeviceMethod: cloudToDeviceMethod);
            // Console.WriteLine($"Client answered {result.GetPayloadAsJson()}");


            var jobClient = JobClient.CreateFromConnectionString(connectionString);
            var jobs = (await registryManager.GetJobsAsync()).ToList();

            // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-query-language
            Func<string, string> devicesInCountry = (country) => $"SELECT * FROM devices WHERE tags.location.region = '{country}'";
            Func<string> allDevices = () => "SELECT * FROM devices";
            Func<IEnumerable<string>, string> allDevicesInGroup = (deviceIds) => $"deviceId IN [{  string.Join(",", deviceIds.Select(_ => $"'{_}'").ToArray()) }]";

            var jobId = Guid.NewGuid().ToString();
            var jobResponse = await jobClient.ScheduleDeviceMethodAsync(
                jobId: jobId,
                // queryCondition: devicesInCountry("Germany"),
                // queryCondition: allDevicesInGroup(new[] { deviceId }),
                queryCondition: allDevices(),
                cloudToDeviceMethod: cloudToDeviceMethod,
                startTimeUtc: DateTime.UtcNow.AddSeconds(1),
                maxExecutionTimeInSeconds: (long)TimeSpan.FromMinutes(4).TotalSeconds);

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                jobResponse = await jobClient.GetJobAsync(jobId);

                Console.WriteLine($"{jobResponse.Status}");
            }
        }
    }
}