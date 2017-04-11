
# Azure IoT Hub Sample

## Interact with IoT Hub via node.js-based iothub-explorer

```cmd
setx AZURE_IOT_HUB_OWNER_KEY HostName=myhub.azure-devices.de;SharedAccessKeyName=iothubowner;SharedAccessKey=DEADBEEFpqE2jWJEz3jFidschiFuckfuck2vjLgnqTocA=
npm install -g iothub-explorer
iothub-explorer.cmd login %azure_iot_hub_owner_key%
```

## Code

- `DotnetProvisionDevice\Provisioner.cs`
	- an ASP.NET WebAPI Endpoint where devices connect and retrieve a shared-access signature to call into IoT hub. 
	- If the requesting device is not yet provisioned in IoT hub, is also creates the entry, and sets metadata
- `DotnetSimulatedDevice\Device.cs`
    - Simulates a device. 
    - Fetches login-credentials from the provisioner. 
    - Actively listens for direct method calls. Implements a direct method called `'some.method'`. 
- `DotnetBackend\Backend.cs`
    - Once the device is running, the Backend calls direct method `'some.method'`. 

## Direct method call 

### Backend code

```csharp
var deviceId = "id00001";
var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
var cloudToDeviceMethod = new CloudToDeviceMethod(methodName: "some.method");
cloudToDeviceMethod.SetPayloadJson(@"{ 
    fwPackageUri : 'https://someurl' 
}");

var result = await serviceClient.InvokeDeviceMethodAsync(deviceId: deviceId, cloudToDeviceMethod: cloudToDeviceMethod);
Console.WriteLine($"Client answered {result.GetPayloadAsJson()}");
```

### Device code

```csharp
{
    var client = DeviceClient.Create(...);
    await client.SetMethodHandlerAsync("some.method", SomeMethodAsync, null);
}

static async Task<MethodResponse> SomeMethodAsync(
    MethodRequest methodRequest, object userContext)
{
    Console.WriteLine($"Device received { methodRequest.DataAsJson }");

    var result = Encoding.UTF8.GetBytes(@"{ 
        'reply': 'Hallo' 
    }");

    return new MethodResponse(result, 200);
}
```

It has to be noted that the `MethodResponse` has a misleading API. It accepts arbitrary `byte[]`s, but [internally checks for JSON](https://github.com/Azure/azure-iot-sdk-csharp/blob/master/device/Microsoft.Azure.Devices.Client/MethodResponse.cs#L48). 

## Invoke multiple devices as part of a job

Check the [query language](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-query-language) for details. Important to not that the query within the Job API does not contain the `SELECT * FROM devices WHERE` part, but only the `tags.location.region = 'Germany'` stuff, i.e. the WHERE clause contents: 

```csharp
var connectionString = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");
var jobClient = JobClient.CreateFromConnectionString(connectionString);
var jobs = (await registryManager.GetJobsAsync()).ToList();

// 
Func<string, string> devicesInCountry = (country) => $"tags.location.region = '{country}'";

var jobId = Guid.NewGuid().ToString();
var jobResponse = await jobClient.ScheduleDeviceMethodAsync(
        jobId: jobId,
        queryCondition: devicesInCountry("Germany"),
        cloudToDeviceMethod: new CloudToDeviceMethod(methodName: "some.method"),
        startTimeUtc: DateTime.UtcNow.AddSeconds(1),
        maxExecutionTimeInSeconds: (long)TimeSpan.FromMinutes(4).TotalSeconds);

while (true)
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    jobResponse = await jobClient.GetJobAsync(jobId);

    Console.WriteLine($"{jobResponse.Status}");
}
```

