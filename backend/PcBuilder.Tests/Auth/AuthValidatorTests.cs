using Auth.Application.Commands;
using Auth.Application.Validators;

namespace PcBuilder.Tests.Auth
{
    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "Password1");
            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_EmptyUsername_ShouldFail()
        {
            var command = new RegisterCommand("", "john@example.com", "Password1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Username");
        }

        [Fact]
        public void Validate_InvalidUsernameCharacters_ShouldFail()
        {
            var command = new RegisterCommand("John Doe!", "john@example.com", "Password1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Username");
        }

        [Fact]
        public void Validate_InvalidEmail_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "not-an-email", "Password1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmptyPassword_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void Validate_ShortPassword_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "Pass1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void Validate_PasswordNoUppercase_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "password1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void Validate_PasswordNoLowercase_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "PASSWORD1");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void Validate_PasswordNoDigit_ShouldFail()
        {
            var command = new RegisterCommand("JohnDoe", "john@example.com", "Password");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }
    }

    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new LoginCommand("john@example.com", "password");
            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_EmptyEmail_ShouldFail()
        {
            var command = new LoginCommand("", "password");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmptyPassword_ShouldFail()
        {
            var command = new LoginCommand("john@example.com", "");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void Validate_BothEmpty_ShouldFailWithMultipleErrors()
        {
            var command = new LoginCommand("", "");
            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }
    }
}
