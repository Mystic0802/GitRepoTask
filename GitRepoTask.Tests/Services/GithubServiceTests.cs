using GitRepoTask.Models;
using GitRepoTask.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace GitRepoTask.Tests.Services
{
    [TestClass]
    public class GithubServiceTests
    {
        private Mock<IRestService> _mockRestService;
        private Mock<ILoggingService> _mockLoggingService;
        private MemoryCache _cache;

        [TestInitialize]
        public void Setup()
        {
            _mockRestService = new Mock<IRestService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _cache = new MemoryCache(Guid.NewGuid().ToString());
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WithNullUsername_ReturnsNull()
        {
            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);

            var result = await githubService.GetProfileDetailsAsync(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WhenUserProfileNotFound_LogsInfoAndReturnsNull()
        {
            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync((GithubUserResponse)null);

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "nonexistent";

            var result = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNull(result);
            _mockLoggingService.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains(username))), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WhenProfileHasNoReposUrl_LogsInfoAndReturnsNull()
        {
            var userResponse = new GithubUserResponse
            {
                name = "Test User",
                location = "Test Location",
                avatar_url = "http://avatar.url",
                repos_url = null // Missing repos_url
            };

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync(userResponse);

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "testuser";

            var result = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNull(result);
            _mockLoggingService.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains(username))), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WithValidProfileAndEmptyRepos_ReturnsProfileWithEmptyRepos()
        {
            var userResponse = new GithubUserResponse
            {
                name = "Test User",
                location = "Test Location",
                avatar_url = "http://avatar.url",
                repos_url = "http://api.github.com/users/testuser/repos"
            };

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync(userResponse);

            _mockRestService
                .Setup(s => s.GetAsync<List<GithubRepoResponse>>(It.IsAny<Uri>()))
                .ReturnsAsync(new List<GithubRepoResponse>());

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "testuser";

            var result = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test User", result.Name);
            Assert.AreEqual("Test Location", result.Location);
            Assert.AreEqual("http://avatar.url", result.AvatarUrl);
            Assert.IsNotNull(result.Repos);
            Assert.AreEqual(0, result.Repos.Count);

            _mockLoggingService.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("No repositories found"))), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WithValidProfileAndRepos_ReturnsSortedTopFiveRepos()
        {
            var userResponse = new GithubUserResponse
            {
                name = "Test User",
                location = "Test Location",
                avatar_url = "http://avatar.url",
                repos_url = "http://api.github.com/users/testuser/repos"
            };

            var repoList = new List<GithubRepoResponse>
            {
                new GithubRepoResponse { Stargazers_Count = 10 },
                new GithubRepoResponse { Stargazers_Count = 50 },
                new GithubRepoResponse { Stargazers_Count = 20 },
                new GithubRepoResponse { Stargazers_Count = 30 },
                new GithubRepoResponse { Stargazers_Count = 40 },
                new GithubRepoResponse { Stargazers_Count = 5 },
                new GithubRepoResponse { Stargazers_Count = 60 }
            };

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync(userResponse);

            _mockRestService
                .Setup(s => s.GetAsync<List<GithubRepoResponse>>(It.IsAny<Uri>()))
                .ReturnsAsync(repoList);

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "testuser";

            var result = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test User", result.Name);
            Assert.AreEqual("Test Location", result.Location);
            Assert.AreEqual("http://avatar.url", result.AvatarUrl);
            Assert.IsNotNull(result.Repos);
            var expectedSortedRepos = new List<GithubRepoResponse>{
                new GithubRepoResponse { Stargazers_Count = 60 },
                new GithubRepoResponse { Stargazers_Count = 50 },
                new GithubRepoResponse { Stargazers_Count = 40 },
                new GithubRepoResponse { Stargazers_Count = 30 },
                new GithubRepoResponse { Stargazers_Count = 20 }
             };
            Assert.AreEqual(5, result.Repos.Count);
            CollectionAssert.AreEqual(
                expectedSortedRepos.Select(r => r.Stargazers_Count).ToList(),
                result.Repos.Select(r => r.Stargazers_Count).ToList(),
                "Expected repositories to be sorted descending by stargazer count and limited to 5 items.");
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_WithValidProfileAndLessThan5Repos_ReturnsSorted3Repos()
        {
            var userResponse = new GithubUserResponse
            {
                name = "Test User",
                location = "Test Location",
                avatar_url = "http://avatar.url",
                repos_url = "http://api.github.com/users/testuser/repos"
            };

            var repoList = new List<GithubRepoResponse>
            {
                new GithubRepoResponse { Stargazers_Count = 20 },
                new GithubRepoResponse { Stargazers_Count = 5 },
                new GithubRepoResponse { Stargazers_Count = 60 }
            };

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync(userResponse);

            _mockRestService
                .Setup(s => s.GetAsync<List<GithubRepoResponse>>(It.IsAny<Uri>()))
                .ReturnsAsync(repoList);

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "testuser";

            var result = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test User", result.Name);
            Assert.AreEqual("Test Location", result.Location);
            Assert.AreEqual("http://avatar.url", result.AvatarUrl);
            Assert.IsNotNull(result.Repos);
            var expectedSortedRepos = new List<GithubRepoResponse>{
                new GithubRepoResponse { Stargazers_Count = 60 },
                new GithubRepoResponse { Stargazers_Count = 20 },
                new GithubRepoResponse { Stargazers_Count = 5 }
             };
            Assert.AreEqual(3, result.Repos.Count);
            CollectionAssert.AreEqual(
                expectedSortedRepos.Select(r => r.Stargazers_Count).ToList(),
                result.Repos.Select(r => r.Stargazers_Count).ToList(),
                "Expected repositories to be sorted descending by stargazer count and still contain only 3 items.");
        }

        [TestMethod]
        public async Task GetProfileDetailsAsync_UsesCache_OnSubsequentCalls()
        {
            var userResponse = new GithubUserResponse
            {
                name = "Test User",
                location = "Test Location",
                avatar_url = "http://avatar.url",
                repos_url = "http://api.github.com/users/testuser/repos"
            };

            var repoList = new List<GithubRepoResponse>
            {
                new GithubRepoResponse { Stargazers_Count = 25 }
            };

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync(userResponse);

            _mockRestService
                .Setup(s => s.GetAsync<List<GithubRepoResponse>>(It.IsAny<Uri>()))
                .ReturnsAsync(repoList);

            var githubService = new GithubService(_mockRestService.Object, _mockLoggingService.Object, _cache);
            string username = "testuser";

            var result1 = await githubService.GetProfileDetailsAsync(username);

            _mockRestService
                .Setup(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()))
                .ReturnsAsync((GithubUserResponse)null);

            var result2 = await githubService.GetProfileDetailsAsync(username);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(result1.Name, result2.Name);

            _mockRestService.Verify(s => s.GetAsync<GithubUserResponse>(It.IsAny<Uri>()), Times.Once);
        }
    }
}