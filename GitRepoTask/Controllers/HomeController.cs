using GitRepoTask.Models;
using GitRepoTask.Services;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GitRepoTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGithubService _githubService;
        private readonly ILoggingService _loggingService;
        private readonly IValidationService _validationService;

        public HomeController(IGithubService githubService, ILoggingService loggingService, IValidationService validationService)
        {
            _githubService = githubService;
            _loggingService = loggingService;
            _validationService = validationService;
        }

        [HttpGet]
        [Route("/")]
        public async Task<ActionResult> Index(string username)
        {
            ViewBag.Title = "Search";

            if (string.IsNullOrWhiteSpace(username))
            {
                return View(new GithubProfileSearchViewModel());
            }

            var viewModel = new GithubProfileSearchViewModel
            {
                SearchModel = new ProfileSearch { Username = username }
            };

            if (!_validationService.IsValidUsername(username))
            {
                ViewBag.Error = "Please enter a valid GitHub username";
                return View(viewModel);
            }

            var result = await GetGithubProfileResultAsync(username);

            if (!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(viewModel);
            }

            viewModel.Profile = result.Profile;
            ViewBag.Title = $"{username}'s Profile";
            ViewBag.Warning = result.WarningMessage;

            return View(viewModel);
        }

        [HttpPost]
        [Route("/")]
        public async Task<ActionResult> Index(GithubProfileSearchViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please enter a valid GitHub username";
                return View(viewModel);
            }

            var result = await GetGithubProfileResultAsync(viewModel.SearchModel.Username);

            if(!result.Success)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(viewModel);
            }

            viewModel.Profile = result.Profile;
            ViewBag.Title = $"{viewModel.SearchModel.Username}'s Profile";
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> GetProfilePartial(string username)
        {
            if (!_validationService.IsValidUsername(username))
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter a valid GitHub username"
                }, JsonRequestBehavior.AllowGet);
            }

            var result = await GetGithubProfileResultAsync(username);

            if(!result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                }, JsonRequestBehavior.AllowGet);
            }

            ViewBag.Warning = result.WarningMessage;
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
                    return GithubProfileResult.CreateFailure("User not found. Please check the username and try again.");
                }

                var result = GithubProfileResult.CreateSuccess(profile);

                if (profile.Repos == null || profile.Repos.Count == 0)
                {
                    result.WarningMessage = "This user doesn't have any public repositories.";
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving profile for {username}", ex);
                return GithubProfileResult.CreateFailure("An unexpected error occurred! Please try again later.");
            }
        }

    }
}