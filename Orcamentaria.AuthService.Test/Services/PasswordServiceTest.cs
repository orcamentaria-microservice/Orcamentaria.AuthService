using FluentAssertions;
using Orcamentaria.AuthService.Application.Services;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    public class PasswordServiceTest
    {
        private readonly PasswordService _service;

        public PasswordServiceTest()
        {
            _service = new PasswordService();
        }

        #region Encript

        [Fact]
        public void Encript_WhenCalled_ReturnsSaltAndHashSeparatedByColon()
        {
            var password = "MinhaSenha123!";

            var result = _service.Encript(password);

            var parts = result.Split(':');
            parts.Should().HaveCount(2);

            Action saltDecode = () => Convert.FromBase64String(parts[0]);
            Action hashDecode = () => Convert.FromBase64String(parts[1]);

            saltDecode.Should().NotThrow();
            hashDecode.Should().NotThrow();
        }

        [Fact]
        public void Encript_WhenCalledTwiceWithSamePassword_ReturnsDifferentResults()
        {
            var password = "MinhaSenha123!";

            var result1 = _service.Encript(password);
            var result2 = _service.Encript(password);

            result1.Should().NotBe(result2);
        }

        #endregion

        #region PasswordIsValid

        [Fact]
        public void PasswordIsValid_WhenPasswordMatchesEncriptedPassword_ReturnsTrue()
        {
            var password = "MinhaSenha123";
            var encripted = _service.Encript(password);

            var result = _service.PasswordIsValid(password, encripted);

            result.Should().BeTrue();
        }

        [Fact]
        public void PasswordIsValid_WhenPasswordDoesNotMatchEncriptedPassword_ReturnsFalse()
        {
            var encripted = _service.Encript("MinhaSenha123");

            var result = _service.PasswordIsValid("OutraSenha456", encripted);

            result.Should().BeFalse();
        }

        [Xunit.Theory]
        [InlineData("semDoisPontosAqui")]
        [InlineData("parte1:parte2:parte3")]
        public void PasswordIsValid_WhenPasswordEncriptIsMalformed_ReturnsFalse(string passwordEncript)
        {
            var result = _service.PasswordIsValid("MinhaSenha123", passwordEncript);

            result.Should().BeFalse();
        }

        #endregion

        #region ValidatePattern

        [Fact]
        public void ValidatePattern_WhenPasswordIsValid_ReturnsIsValidTrue()
        {
            var password = "Abcdef123!";

            var result = _service.ValidatePattern(password);

            result.IsValid.Should().BeTrue();
        }

        [Xunit.Theory]
        [InlineData("Ab1!")]
        [InlineData("abcdefgh123!")]
        [InlineData("Abcdefgh12!")]
        [InlineData("Abcdefgh123")]
        [InlineData("Abc def123!")]
        public void ValidatePattern_WhenPasswordIsInvalid_ReturnsIsValidFalseWithErrors(string password)
        {
            var result = _service.ValidatePattern(password);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        #endregion
    }
}
