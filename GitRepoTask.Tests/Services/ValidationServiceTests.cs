using Microsoft.VisualStudio.TestTools.UnitTesting;
using GitRepoTask.Services;

namespace GitRepoTask.Tests.Services
{
    [TestClass]
    public class ValidationServiceTests
    {
        private IValidationService _validationService;

        [TestInitialize]
        public void Setup()
        {
            _validationService = new ValidationService();
        }

        [TestMethod]
        public void IsValidUsername_WithValidUsername_ReturnsTrue()
        {
            var validUsername = "ValidUser123";

            var result = _validationService.IsValidUsername(validUsername);
            
            Assert.IsTrue(result, "Expected a valid username to return true.");
        }

        [TestMethod]
        public void IsValidUsername_WithEmptyUsername_ReturnsFalse()
        {
            var username = "";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected an empty username to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithWhitespaceUsername_ReturnsFalse()
        {
            var username = "    ";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected a whitespace-only username to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithUsernameExceedingMaxLength_ReturnsFalse()
        {
            var username = new string('a', 40);

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected a username exceeding max length to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithUsernameStartingWithHyphen_ReturnsFalse()
        {
            var username = "-InvalidUser";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected a username starting with a hyphen to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithUsernameEndingWithHyphen_ReturnsFalse()
        {
            var username = "InvalidUser-";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected a username ending with a hyphen to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithUsernameHavingConsecutiveHyphens_ReturnsFalse()
        {
            var username = "Invalid--User";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsFalse(result, "Expected a username with consecutive hyphens to return false.");
        }

        [TestMethod]
        public void IsValidUsername_WithUsernameContainingValidHyphen_ReturnsTrue()
        {
            var username = "Valid-User";

            var result = _validationService.IsValidUsername(username);
            
            Assert.IsTrue(result, "Expected a username with a valid hyphen to return true.");
        }
    }
}