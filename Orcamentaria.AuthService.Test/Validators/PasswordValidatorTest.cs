using FluentAssertions;
using Orcamentaria.AuthService.Application.Validators;
using Xunit;

namespace Orcamentaria.AuthService.Test.Validators
{
    public class PasswordValidatorTest
    {
        private readonly PasswordValidator _validator;

        public PasswordValidatorTest()
        {
            _validator = new PasswordValidator();
        }

        #region ValidatePattern

        [Fact]
        public void ValidatePattern_WhenPasswordIsValid_ReturnsIsValid()
        {
            var result = _validator.ValidatePattern("Abcdef123!");

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidatePattern_WhenPasswordIsEmpty_ReturnsInvalid()
        {
            var result = _validator.ValidatePattern(string.Empty);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage.Contains("deve ser informado."));
        }

        [Xunit.Theory]
        [InlineData("abcdef123!")]
        [InlineData("Abcdefgh!")]
        [InlineData("Abc12!")]
        [InlineData("Abcdefg123")]
        [InlineData("Abc def123!")]
        public void ValidatePattern_WhenPasswordDoesNotMatchPattern_ReturnsInvalid(string password)
        {
            var result = _validator.ValidatePattern(password);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage.Contains("Password deve ter pelo menos 8 caracteres"));
        }

        #endregion
    }
}
