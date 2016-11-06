using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal static class Telemetry
    {
        private const string c_InstrumentationKey = "243085e4-077a-429b-9c8c-4acf793eefaa";

        public static readonly TelemetryClient Client = CreateClient();

        private static TelemetryClient CreateClient()
        {
            var configuration = new TelemetryConfiguration
            {
                InstrumentationKey = c_InstrumentationKey,
                TelemetryChannel = new InMemoryChannel
                {
#if DEBUG
                    DeveloperMode = true
#else
                    DeveloperMode = false
#endif
                }
            };

            var client = new TelemetryClient(configuration);
            client.Context.User.Id = Anonymize(Environment.UserDomainName + "\\" + Environment.UserName);
            client.Context.Session.Id = Guid.NewGuid().ToString();
            client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            client.Context.Component.Version = typeof(Telemetry).Assembly.GetName().Version.ToString();

            return client;
        }

        private static string Anonymize(string str)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.Unicode.GetBytes(str);
                byte[] hash = sha1.ComputeHash(inputBytes);
                string base64 = Convert.ToBase64String(hash);
                return base64;
            }
        }

        /// <summary>
        /// Helper to create a property dictionary with a single key/value pair.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>A dictioanry with a single key/value pair.</returns>
        public static IDictionary<string, string> CreateProperties(string key, string value)
        {
            var retVal = new Dictionary<string,string>();
            retVal.Add(key, value);
            return retVal;
        }
    }
}
