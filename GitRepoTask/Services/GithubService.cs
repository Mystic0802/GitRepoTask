using GitRepoTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace GitRepoTask.Services
{
    public interface IGithubService
    {
        Task<GithubProfile> GetProfileDetailsAsync(string username);
    }

    public class GithubService : IGithubService
    {
        const string GITHUB_API_URL = "https://api.github.com/users/";

        private readonly IRestService _restService;
        private readonly ILoggingService _loggingService;
        private readonly MemoryCache _cache;

        public GithubService(IRestService restService, ILoggingService loggingService, MemoryCache memoryCache)
        {
            _restService = restService;
            _loggingService = loggingService;
            _cache = memoryCache;
        }

        public async Task<GithubProfile> GetProfileDetailsAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) 
                return null;

            string cacheKey = $"GithubProfile_{username}";
            if (_cache.Contains(cacheKey))
            {
                return (GithubProfile)_cache.Get(cacheKey);
            }

            var profile = await GetProfileAsync(username);
            if (profile == null || string.IsNullOrEmpty(profile.repos_url))
            {
                _loggingService.LogInfo($"No profile (or repos_url) found for username: {username}");
                return null;
            }

            var repos = await GetGitHubRepoListAsync(profile.repos_url);

            if (repos == null || !repos.Any())
            {
                _loggingService.LogInfo($"No repositories found for username: {username}");
            }

            var githubProfile = new GithubProfile
            {
                Username = username,
                Name = profile.name,
                Location = profile.location ?? "Unknown",
                AvatarUrl = profile.avatar_url ?? "",
                Repos = repos.OrderByDescending(r => r.Stargazers_Count)
                    .Take(5)
                    .ToList() ?? new List<GithubRepoResponse>()
            };

            _cache.Set(cacheKey, githubProfile, DateTimeOffset.Now.Add(TimeSpan.FromHours(1)));

            return githubProfile;
        }

        private async Task<GithubUserResponse> GetProfileAsync(string username)
        {
            return await _restService.GetAsync<GithubUserResponse>(new Uri(GITHUB_API_URL + username));
        }

        private async Task<List<GithubRepoResponse>> GetGitHubRepoListAsync(string apiUrl)
        {
            if (string.IsNullOrEmpty(apiUrl))
                return null;
            return await _restService.GetAsync<List<GithubRepoResponse>>(new Uri(apiUrl));

        }
    }
}