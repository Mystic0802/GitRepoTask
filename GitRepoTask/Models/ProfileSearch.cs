using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GitRepoTask.Models
{
    public class ProfileSearch
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z0-9](?:[a-zA-Z0-9]|-(?=[a-zA-Z0-9])){0,38}$",
            ErrorMessage = "Username can only contain alphanumeric characters and single hyphens")]
        [StringLength(39, MinimumLength = 1,
            ErrorMessage = "Username must be between 1 and 39 characters")]
        public string Username { get; set; }
    }
}