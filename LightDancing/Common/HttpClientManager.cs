using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightDancing.Common
{
    public class HttpClientManager
    {
        private HttpClient _httpClient;
        private string _domain;

        public HttpClientManager(string domain)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _domain = domain;
        }

        public void ChangeDomain(string newDomain)
        {
            _domain = newDomain;
        }

        public T GetAsync<T>(string endpoint)
        {
            var response = Task.Run(() => _httpClient.GetAsync(new Uri($"{_domain}{endpoint}"))).Result;
            response.EnsureSuccessStatusCode();
            string content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(content);
        }

        public T PostAsync<T>(string endpoint, string content = null, string contentType = "application/json")
        {
            HttpContent httpContent = content != null ? new StringContent(content, Encoding.UTF8, contentType) : new ByteArrayContent(new byte[0]);

            var response = Task.Run(() => _httpClient.PostAsync(new Uri($"{_domain}{endpoint}"), httpContent)).Result;
            response.EnsureSuccessStatusCode();
            string responseContent = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        public T PutAsync<T>(string endpoint, string content, string contentType = "application/json")
        {
            var httpContent = new StringContent(content, Encoding.UTF8, contentType);
            var response = Task.Run(() => _httpClient.PutAsync(new Uri($"{_domain}{endpoint}"), httpContent)).Result;
            response.EnsureSuccessStatusCode();
            string responseContent = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        public void PutAsyncNotAwait(string endpoint, string content, string contentType = "application/json")
        {
            var httpContent = new StringContent(content, Encoding.UTF8, contentType);
            var response = Task.Run(() => _httpClient.PutAsync(new Uri($"{_domain}{endpoint}"), httpContent));
        }
    }
}