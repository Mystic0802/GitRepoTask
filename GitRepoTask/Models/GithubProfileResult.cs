using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitRepoTask.Models
{
    public class GithubProfileResult
    {
        public bool Success { get; private set; }
        public GithubProfile Profile { get; private set; }
        public string ErrorMessage { get; private set; }
        public string WarningMessage { get; set; }

        private GithubProfileResult() { }

        public static GithubProfileResult CreateSuccess(GithubProfile profile)
        {
            return new GithubProfileResult
            {
                Success = true,
                Profile = profile
            };
        }

        public static GithubProfileResult CreateFailure(string errorMessage)
        {
            return new GithubProfileResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}