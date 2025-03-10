namespace GitRepoTask.Models
{
    public class GithubRepoResponse
    {
        public string Name { get; set; }
        public string Html_url { get; set; }
        public string Description { get; set; }
        public int Stargazers_Count { get; set; }
    }
}