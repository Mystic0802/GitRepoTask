using GitRepoTask.Controllers;
using GitRepoTask.Models;
using GitRepoTask.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GitRepoTask.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<IGithubService> _mockGithubService;
        private Mock<ILoggingService> _mockLoggingService;
        private HomeController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockGithubService = new Mock<IGithubService>();
            _mockLoggingService = new Mock<ILoggingService>();
            _controller = new HomeController(_mockGithubService.Object, _mockLoggingService.Object);

            System.Web.HttpContext.Current = new System.Web.HttpContext(
                new System.Web.HttpRequest("", "http://tempuri.org", ""),
                new System.Web.HttpResponse(new StringWriter()));

            var controllerContext = new ControllerContext();
            controllerContext.HttpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            _controller.ControllerContext = controllerContext;
        }

        [TestMethod]
        public async Task Index_Returns_View_With_ProfileSearch_Model()
        {
            var result = _controller.Index() as ViewResult;

            Assert.IsNotNull(result, "Expected a ViewResult");
            Assert.IsInstanceOfType(result.Model, typeof(ProfileSearch), "Expected the model to be of type ProfileSearch");
        }

        [TestMethod]
        public async Task GetProfilePartial_InvalidModelState_Returns_BadRequest()
        {
            _controller.ModelState.AddModelError("Username", "Required");
            var profileSearch = new ProfileSearch();

            var result = await _controller.GetProfilePartial(profileSearch) as ContentResult;


            Assert.AreEqual(400, _controller.Response.StatusCode, "Expected status code 400 for invalid model state");
            Assert.AreEqual("Please enter a valid GitHub username", result.Content, "Expected error message for invalid username");
        }

        [TestMethod]
        public async Task GetProfilePartial_ProfileNotFound_Returns_NotFound()
        {
            string username = "nonexistent";
            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username))
                .ReturnsAsync((GithubProfile)null);
            var profileSearch = new ProfileSearch { Username = username };

            var result = await _controller.GetProfilePartial(profileSearch) as ContentResult;

            Assert.AreEqual(404, _controller.Response.StatusCode, "Expected status code 404 when profile is not found");
            Assert.AreEqual("User not found. Please check the username and try again.", result.Content, "Expected not found message");
        }

        [TestMethod]
        public async Task GetProfilePartial_ProfileFound_Returns_PartialView()
        {
            string username = "validUser";
            var expectedProfile = new GithubProfile
            {
                Username = username,
                Name = "Valid User",
                Location = "Somewhere",
                AvatarUrl = "http://avatar.url",
                Repos = new List<GithubRepoResponse>()
            };

            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username))
                    .ReturnsAsync(expectedProfile);
            var profileSearch = new ProfileSearch { Username = username };

            var result = await _controller.GetProfilePartial(profileSearch) as PartialViewResult;

            Assert.IsNotNull(result, "Expected a PartialViewResult when profile is found");
            Assert.AreEqual("_GithubProfile", result.ViewName, "Expected the partial view name to be '_GithubProfile'");
            Assert.AreEqual(expectedProfile, result.Model, "Expected the model to match the returned profile");
        }

        [TestMethod]
        public async Task GetProfilePartial_Exception_Returns_NotFound()
        {
            string username = "errorUser";
            _mockGithubService.Setup(s => s.GetProfileDetailsAsync(username))
                .ThrowsAsync(new Exception("Test exception"));
            var profileSearch = new ProfileSearch { Username = username };

            var result = await _controller.GetProfilePartial(profileSearch) as ContentResult;

            Assert.AreEqual(404, _controller.Response.StatusCode, "Expected status code 404 when an exception occurs");
            Assert.AreEqual("User not found. Please check the username and try again.", result.Content, "Expected error message when an exception is thrown");
        }
    }
}