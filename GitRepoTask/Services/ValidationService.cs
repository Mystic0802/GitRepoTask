using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace GitRepoTask.Services
{
    public interface IValidationService
    {
        bool IsValidUsername(string username);
    }

    public class ValidationService : IValidationService
    {
        private const int MaxUsernameLength = 39;
        private static readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9](?:[a-zA-Z0-9]|-(?=[a-zA-Z0-9])){0,38}$", RegexOptions.Compiled); // alphanumeric, max 39 characters, no consecutive hyphens or hyphens at the start or end

        public bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (username.Length > MaxUsernameLength)
            {
                return false;
            }

            return UsernameRegex.IsMatch(username);
        }
    }
}