using System;
using System.Net;
using System.Net.Http;

namespace ModAssistant
{
    internal static class Http
    {
        private static HttpClient? _client;

        public static HttpClient HttpClient
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }

                HttpClientHandler? handler = new()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };

                _client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(240),
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _client.DefaultRequestHeaders.Add("User-Agent", "ModAssistant/" + App.Version);

                return _client;
            }
        }
    }
}
