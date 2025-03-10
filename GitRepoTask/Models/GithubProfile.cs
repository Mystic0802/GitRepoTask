using System.Collections.Generic;

namespace GitRepoTask.Models
{
    public class GithubProfile
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string AvatarUrl { get; set; }
        public List<GithubRepoResponse> Repos { get; set; } = new List<GithubRepoResponse>();
    }
}