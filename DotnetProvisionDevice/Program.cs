using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetProvisionDevice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Device Provisioning";

            var key = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");
            var deviceKeyId = "id:device:000001";
        }
    }
}
