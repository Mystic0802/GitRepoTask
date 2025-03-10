using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitRepoTask.Models
{
    public class GithubProfileSearchViewModel
    {
        public ProfileSearch SearchModel { get; set; }
        public GithubProfile Profile { get; set; }
    }
}