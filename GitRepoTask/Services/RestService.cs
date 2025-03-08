using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace GitRepoTask.Services
{
    public class RestService
    {
        private readonly HttpClient _httpClient;

        public RestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetAsync<T>(Uri apiUrl)
        {
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return default;

            try
            {
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch
            {
                return default;
            }
        }
    }
}