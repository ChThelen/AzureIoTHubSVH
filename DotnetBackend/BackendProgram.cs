using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Backend";
            var key = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_OWNER_KEY");

            Console.WriteLine($"{key}");
        }
    }
}
