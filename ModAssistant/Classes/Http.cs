using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Script.Serialization;
using BrotliSharpLib;
using System.Linq;
using System.IO.Compression;
using System.IO;

namespace ModAssistant
{
    static class Http
    {
        private static HttpClient _client = null;

        public static HttpClient HttpClient
        {
            get
            {
                if (_client != null) return _client;

                /*var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };*/

                _client = new HttpClient(new BrotliCompressionHandler())
                {
                    Timeout = TimeSpan.FromSeconds(360),
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
                _client.DefaultRequestHeaders.Add("User-Agent", "ModAssistant/" + App.Version);

                return _client;
            }
        }

        public static JavaScriptSerializer JsonSerializer = new JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue,
        };

        private class BrotliCompressionHandler : DelegatingHandler
        {
            public BrotliCompressionHandler() : base(new HttpClientHandler())
            {
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Specify supported encodings in the request
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

                return base.SendAsync(request, cancellationToken).ContinueWith(responseTask =>
                {
                    HttpResponseMessage response = responseTask.Result;

                    // Check if the response is compressed
                    if (response.Content.Headers.ContentEncoding != null)
                    {
                        foreach (var encoding in response.Content.Headers.ContentEncoding)
                        {
                            switch (encoding.ToLowerInvariant())
                            {
                                case "br":
                                    // Decompress Brotli content
                                    byte[] brotliCompressedBytes = response.Content.ReadAsByteArrayAsync().Result;
                                    byte[] brotliDecompressedBytes = Brotli.DecompressBuffer(brotliCompressedBytes, 0, brotliCompressedBytes.Length);
                                    response.Content = new ByteArrayContent(brotliDecompressedBytes);
                                    response.Content.Headers.Remove("Content-Encoding");
                                    break;

                                case "gzip":
                                    // Decompress GZip content
                                    using (var stream = new GZipStream(response.Content.ReadAsStreamAsync().Result, CompressionMode.Decompress))
                                    {
                                        var resultStream = new MemoryStream();
                                        stream.CopyTo(resultStream);
                                        response.Content = new ByteArrayContent(resultStream.ToArray());
                                    }
                                    response.Content.Headers.Remove("Content-Encoding");
                                    break;

                                case "deflate":
                                    // Decompress Deflate content
                                    using (var stream = new DeflateStream(response.Content.ReadAsStreamAsync().Result, CompressionMode.Decompress))
                                    {
                                        var resultStream = new MemoryStream();
                                        stream.CopyTo(resultStream);
                                        response.Content = new ByteArrayContent(resultStream.ToArray());
                                    }
                                    response.Content.Headers.Remove("Content-Encoding");
                                    break;
                            }
                        }
                    }

                    return response;
                }, cancellationToken);
            }
        }
    }
}
