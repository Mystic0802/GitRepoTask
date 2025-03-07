using GitRepoTask.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;

namespace GitRepoTask.Services
{

    public class GithubService
    {
        private readonly HttpClient _httpClient;

        public GithubService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
        }

        public async Task<GithubUser> GetGitHubProfileAsync(string username)
        {
            var response = await _httpClient.GetAsync($"users/{username}");

            if (!response.IsSuccessStatusCode)
                return null; // could split into the different codes

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GithubUser>(json);
        }

        public async Task<List<GithubRepo>> GetGitHubRepoListAsync(Uri apiUrl)
        {
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return null; // could split into the different codes

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<GithubRepo>>(json);
        }

        public async Task<T> GetTAsync<T>(Uri apiUrl)
        {
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}