using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitRepoTask.Services
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(Uri apiUrl);
    }

    public class RestService : IRestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;

        public RestService(HttpClient httpClient, ILoggingService loggingService)
        {
            _httpClient = httpClient;
            _loggingService = loggingService;

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitRepoTask");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<T> GetAsync<T>(Uri apiUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return default;

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error while accessing API ({apiUrl}): ", ex);
                return default;
            }
        }

    }
}