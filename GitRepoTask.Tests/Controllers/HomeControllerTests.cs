using GitRepoTask.Controllers;
using GitRepoTask.Models;
using GitRepoTask.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GitRepoTask.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<IGithubService> _mockGithubService;
        private Mock<ILoggingService> _mockLoggingService;
        private Mock<IValidationService> _mockValidationService;
        private HomeController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockGithubService = new Mock<IGithubService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _mockValidationService = new Mock<IValidationService>();
            _controller = new HomeController(_mockGithubService.Object, _mockLoggingService.Object, _mockValidationService.Object);
        }

        [TestMethod]
        public async Task Index_Get_WithNullOrEmptyUsername_ReturnsViewWithEmptyModel()
        {
            string username = null;

            var result = await _controller.Index(username) as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as GithubProfileSearchViewModel;
            Assert.IsNotNull(model, "Expected model to be of type GithubProfileSearchViewModel");
            Assert.IsNull(model.Profile, "Expected profile to be null when username is empty");
        }

        [TestMethod]
        public async Task Index_Get_WithInvalidUsername_ReturnsViewWithErrorMessage()
        {
            string username = "invalidUser!";
            _mockValidationService.Setup(v => v.IsValidUsername(username)).Returns(false);

            var result = await _controller.Index(username) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Please enter a valid GitHub username", result.ViewBag.Error);
        }

        [TestMethod]
        public async Task Index_Get_WithValidUsername_ReturnsViewWithProfile()
        {
            string username = "validUser";
            _mockValidationService.Setup(v => v.IsValidUsername(username)).Returns(true);
            var githubProfile = new GithubProfile
            {
                Name = "Valid User",
                Location = "Somewhere",
                AvatarUrl = "http://avatar.url",
                Repos = new List<GithubRepoResponse>()
            };

            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username)).ReturnsAsync(githubProfile);

            var result = await _controller.Index(username) as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as GithubProfileSearchViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(githubProfile, model.Profile);
            Assert.AreEqual($"{username}'s Profile", result.ViewBag.Title);
        }

        [TestMethod]
        public async Task Index_Post_WithInvalidModelState_ReturnsViewWithErrorMessage()
        {
            _controller.ModelState.AddModelError("Username", "Required");
            var viewModel = new GithubProfileSearchViewModel { SearchModel = new ProfileSearch { Username = "invalidUser" } };

            var result = await _controller.Index(viewModel) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Please enter a valid GitHub username", result.ViewBag.Error);
        }

        [TestMethod]
        public async Task Index_Post_WithValidModel_ReturnsViewWithProfile()
        {
            var username = "validUser";
            var viewModel = new GithubProfileSearchViewModel { SearchModel = new ProfileSearch { Username = username } };

            var githubProfile = new GithubProfile
            {
                Name = "Valid User",
                Location = "Somewhere",
                AvatarUrl = "http://avatar.url",
                Repos = new List<GithubRepoResponse>()
            };

            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username)).ReturnsAsync(githubProfile);

            var result = await _controller.Index(viewModel) as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as GithubProfileSearchViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(githubProfile, model.Profile);
            Assert.AreEqual($"{username}'s Profile", result.ViewBag.Title);
        }

        [TestMethod]
        public async Task GetProfilePartial_WithInvalidUsername_ReturnsJsonError()
        {
            string username = "invalidUser!";
            _mockValidationService.Setup(v => v.IsValidUsername(username)).Returns(false);

            var result = await _controller.GetProfilePartial(username) as JsonResult;

            Assert.IsNotNull(result);
            dynamic data = result.Data;
            Assert.IsFalse(data.success);
            StringAssert.Contains(data.message, "Please enter a valid GitHub username");
        }

        [TestMethod]
        public async Task GetProfilePartial_WithValidUsername_ReturnsPartialViewWithProfile()
        {
            string username = "validUser";
            _mockValidationService.Setup(v => v.IsValidUsername(username)).Returns(true);
            var githubProfile = new GithubProfile
            {
                Name = "Valid User",
                Repos = new List<GithubRepoResponse>()
            };
            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username)).ReturnsAsync(githubProfile);

            var result = await _controller.GetProfilePartial(username) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_GithubProfile", result.ViewName);
            Assert.AreEqual(githubProfile, result.Model);
        }
    }
}