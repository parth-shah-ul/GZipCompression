using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GZipCompression
{
    public class TestClass
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string IPAddress { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Get();
        }

        
        public const string apiURL = "http://localhost:63006/v4?compressed=true";
        public static HttpClient _client;
        public static HttpRequestMessage _requestMessage;
        

        public static async Task<dynamic> Get()
        {
            //Initialize HTTP Client to send API request
            InitializeHttpClient();

            // Receive API response
            var response = _client.SendAsync(_requestMessage).Result; 

            //Check whether response header contains encoding of type gzip; if so, decompress response
            bool isCompressedResponse = response.Content.Headers.GetValues("Content-Encoding").FirstOrDefault() == "gzip";

            if (isCompressedResponse)
            {
                //read response as byte array
                var responseArray = response.Content.ReadAsByteArrayAsync().Result;

                //use memory stream to read the array and decompress
                using (var stream = new GZipStream(new MemoryStream(responseArray), CompressionMode.Decompress))
                {
                    var buffer = new byte[responseArray.Length];
                    using (var memoryStream = new MemoryStream())
                    {
                        int count;
                        do
                        {
                            //keep reading response
                            count = stream.Read(buffer, 0, buffer.Length);
                            if (count > 0)
                            {
                                memoryStream.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);

                        //Convert byte array to any JSON object
                        var output = ByteArrayToObject(memoryStream.ToArray());
                        return output;
                    }
                }
            }

            return response;
        }
        
        public static object ByteArrayToObject(byte[] _ByteArray)
        {
            string jsonStr = Encoding.UTF8.GetString(_ByteArray);
            return JsonConvert.DeserializeObject<List<TestClass>>(jsonStr);
        }

        private static void InitializeHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(apiURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client = client;

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(apiURL));
            requestMessage.Headers.Add("Authorization", "Basic aU9TLjE6MTppRzlEclZwVzRsMHdUbm9paERLbUtmUHlib1laZm5yM00yWHpKeWZJSHBFPQ==");
            _requestMessage = requestMessage;
        }
    }
}
