using System;
using System.Net;
using System.Net.Http;
using System.Web.Script.Serialization;

namespace ModAssistant
{
    static class Http
    {
        private static bool _initCalled = false;

        private static readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        public static readonly HttpClient HttpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        public static JavaScriptSerializer JsonSerializer = new JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue,
        };

        public static void InitClient()
        {
            if (_initCalled == true) return;
            _initCalled = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient.DefaultRequestHeaders.Add("User-Agent", "ModAssistant/" + App.Version);
        }
    }
}
