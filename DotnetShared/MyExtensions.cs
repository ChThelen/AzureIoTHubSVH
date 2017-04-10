namespace DotnetSharedTypes
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    public class IoTHubConnectionStringParams
    {
        public string HostName { get; set; }
        public string SharedAccessKeyName { get; set; }
        public byte[] SharedAccessKey { get; set; }
    }

    public static class MyExtensions
    {
        public static IoTHubConnectionStringParams ParseAzureIoTHubConnectionString(this string connectionString)
        {
            Func<string, Tuple<string,string>> GetKV = kv =>
            {
                var idx = kv.IndexOf('=');
                return new Tuple<string, string>(
                    kv.Substring(0, idx), 
                    kv.Substring(idx + 1));
            };

            var pairs = connectionString.Split(';')
                .Select(GetKV)
                .ToDictionary(_ => _.Item1, _ => _.Item2);

            return new IoTHubConnectionStringParams
            {
                HostName = pairs["HostName"],
                SharedAccessKeyName = pairs["SharedAccessKeyName"],
                SharedAccessKey = Convert.FromBase64String(pairs["SharedAccessKey"])
            };
        }

        public static string GetSASToken(this IoTHubConnectionStringParams hub, string deviceId, TimeSpan duration)
        {
            var baseAddress = $"{hub.HostName}/devices/{deviceId}".ToLower();
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + duration.TotalSeconds);

            string stringToSign = WebUtility.UrlEncode(baseAddress).ToLower() + "\n" + expiry;

            var hmac = new HMACSHA256(hub.SharedAccessKey);
            byte[] sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            return $"SharedAccessSignature sr={WebUtility.UrlEncode(baseAddress).ToLower()}&se={expiry}&sig={WebUtility.UrlEncode(Convert.ToBase64String(sig))}";
        }
    }
}
