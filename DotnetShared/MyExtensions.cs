namespace DotnetShared
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public class IoTHubConnectionStringParams
    {
        public string HostName { get; set; }
        public string SharedAccessKeyName { get; set; }
        public byte[] SharedAccessKey { get; set; }
    }

    public static class MyExtensions
    {
        public static Dictionary<string, string> ParseConnectionString(this string connectionString)
        {
            return connectionString.Split(';')
                .ToDictionary(
                    _ => _.Substring(0, _.IndexOf('=')), 
                    _ => _.Substring(_.IndexOf('=') + 1));
        }

        public static string GetConnectionStringParameter(this string connectionString, string key)
        {
            return connectionString.ParseConnectionString()[key];
        }

        public static string GetSASToken(string hubHostname, string deviceId, byte[] deviceKey, TimeSpan duration)
        {
            var sr = WebUtility.UrlEncode($"{hubHostname}/devices/{deviceId}".ToLower());

            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var se = Convert.ToString((long)(fromEpochStart.TotalSeconds + duration.TotalSeconds));

            var stringToSign = $"{sr}\n{se}";
            var hmac = new HMACSHA256(deviceKey);
            var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            var sig = WebUtility.UrlEncode(Convert.ToBase64String(sigBytes));

            return $"SharedAccessSignature sr={sr}&sig={sig}&se={se}"; // + $"&skn={policyName}";
        }

        public static async Task<DeviceCredentials> GetDeviceCredentialsAsync(string deviceIdentifier)
        {
            using (var httpClient = new HttpClient { BaseAddress = new Uri(Constants.ProvisioningServer) })
            {
                var msg = await httpClient.GetAsync($"{Constants.ProvisioningServerPath}/{deviceIdentifier}");
                var readTokenTask = await msg.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DeviceCredentials>(readTokenTask);
            }
        }
    }
}