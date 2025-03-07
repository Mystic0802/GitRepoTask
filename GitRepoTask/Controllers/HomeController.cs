using GitRepoTask.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GitRepoTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly GithubService _githubService;

        public HomeController(GithubService githubService)
        {
            _githubService = githubService;
        }

        [Route("/")]
        public async Task<ActionResult> Index(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Title = "Search";
                return View();
            }
            var profile = await _githubService.GetGitHubProfileAsync(username);
            ViewBag.Title = $"{username}'s Profile";
            return View(profile);
        }

        [Route("/Result")]
        public ActionResult Result()
        {

            return View();
        }
    }
}