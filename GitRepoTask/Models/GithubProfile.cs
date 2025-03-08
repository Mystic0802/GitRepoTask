using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitRepoTask.Models
{
    public class GithubProfile
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string AvatarUrl { get; set; }
        public List<GithubRepo> Repos { get; set; } = new List<GithubRepo>();
    }
}