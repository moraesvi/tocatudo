using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class HttpApiHelper
    {
        private static readonly int _maxRetryAttempts = 3;
        private static readonly TimeSpan _pauseBetweenFailures = TimeSpan.FromSeconds(2);
        private static readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(_maxRetryAttempts, i => _pauseBetweenFailures);
        private static readonly HttpClient _httpClient = new HttpClient();
        public static async Task<T> Get<T>(string url) where T : class
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage httpResp = await _httpClient.GetAsync(url);

                if (httpResp.IsSuccessStatusCode)
                {
                    using (StreamReader stream = new StreamReader(await httpResp.Content.ReadAsStreamAsync()))
                    {
                        string sJson = await stream.ReadToEndAsync();
                        return JsonConvert.DeserializeObject<T>(sJson);
                    }
                }

                return default(T);
            });
        }
        public static async Task<T> Post<T>(string url, object objRequest)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string jsonRequest = JsonConvert.SerializeObject(objRequest);
                StringContent requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResp = await _httpClient.PostAsync(url, requestContent);

                if (httpResp.IsSuccessStatusCode)
                {
                    using (StreamReader stream = new StreamReader(await httpResp.Content.ReadAsStreamAsync()))
                    {
                        string sJson = await stream.ReadToEndAsync();
                        return JsonConvert.DeserializeObject<T>(sJson);
                    }
                }

                return default(T);
            });
        }
    }
}
