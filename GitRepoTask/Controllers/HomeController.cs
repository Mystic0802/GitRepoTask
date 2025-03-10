using GitRepoTask.Models;
using GitRepoTask.Services;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GitRepoTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGithubService _githubService;
        private readonly ILoggingService _loggingService;

        public HomeController(IGithubService githubService, ILoggingService loggingService)
        {
            _githubService = githubService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [Route("/")]
        public ViewResult Index()
        {
            ViewBag.Title = "Search";

            return View(new ProfileSearch());
        }

        [HttpGet]
        public async Task<ActionResult> GetProfilePartial(ProfileSearch profileSearch)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Content("Please enter a valid GitHub username");
            }

            var username = profileSearch.Username;

            var result = await GetGithubProfileResultAsync(username);

            if(!result.Success)
            {
                Response.StatusCode = 404;
                return Content(result.ErrorMessage);
            }

            return PartialView("_GithubProfile", result.Profile);
        }

        private async Task<GithubProfileResult> GetGithubProfileResultAsync(string username)
        {
            _loggingService.LogInfo($"Searching for GitHub user: {username}");

            try
            {
                var profile = await _githubService.GetProfileDetailsAsync(username);

                if (profile == null)
                {
                    return new GithubProfileResult() 
                    { 
                        Success = false,
                        ErrorMessage = "User not found. Please check the username and try again." };
                }

                var result = new GithubProfileResult() {
                    Success = true,
                    Profile = profile
                };

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving profile for {username}", ex);
                return new GithubProfileResult() { Success = false, ErrorMessage = "User not found. Please check the username and try again." };
            }
        }

    }
}