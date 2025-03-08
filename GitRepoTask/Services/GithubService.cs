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
    public interface IGithubService
    {
        Task<GithubProfile> GetProfileDetailsAsync(string username);
    }

    public class GithubService : IGithubService
    {
        private readonly HttpClient _httpClient;
        private readonly RestService _restService;

        public GithubService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _restService = new RestService(_httpClient);
        }

        public async Task<GithubProfile> GetProfileDetailsAsync(string username)
        {
            var profile = await GetProfileAsync(username);
            if (profile == null || string.IsNullOrEmpty(profile.repos_url))
                return null;

            var repos = await GetGitHubRepoListAsync(profile.repos_url);

            return new GithubProfile
            {
                Name = profile.name,
                Location = profile.location,
                AvatarUrl = profile.avatar_url,
                Repos = repos.Select(r => new GithubRepo
                {
                    Name = r.name,
                    Description = r.description,
                    Url = r.html_url
                }).OrderByDescending(r => r.StargazersCount)
                .Take(5)
                .ToList()
            };
        }

        private async Task<GithubUserResponse> GetProfileAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return await _restService.GetAsync<GithubUserResponse>(new Uri($"users/{username}", UriKind.Relative));
        }

        private async Task<List<GithubRepoResponse>> GetGitHubRepoListAsync(string apiUrl)
        {
            if (string.IsNullOrEmpty(apiUrl))
                return null;
            return await _restService.GetAsync<List<GithubRepoResponse>>(new Uri(apiUrl));

        }
    }
}