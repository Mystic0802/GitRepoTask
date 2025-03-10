using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitRepoTask.Models
{
    public class GithubProfileResult
    {
        public bool Success { get; set; }
        public GithubProfile Profile { get;  set; }
        public string ErrorMessage { get; set; }

        public GithubProfileResult() { }
    }
}