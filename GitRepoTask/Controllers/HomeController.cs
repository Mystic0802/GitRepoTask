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
        private readonly IGithubService _githubService;

        public HomeController(IGithubService githubService)
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
            // add some kind of username sanitisation
            // sanitise(username);
            var profile = await _githubService.GetProfileDetailsAsync(username);
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