using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(UserCollection))]
    public class UserRefreshTokenServiceTest
    {
        private readonly UserFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.UserRefreshTokenService _service;

        public UserRefreshTokenServiceTest(UserFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.UserRefreshTokenService>(true);
        }

        #region Generate

        [Fact]
        public void Generate_WhenValidData_ReturnsValidJwt()
        {
            var rsa = RSA.Create(2048);
            var securityKey = new RsaSecurityKey(rsa);
            var user = _fixture.CreateEntity(1);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(securityKey);

            var token = _service.Generate(user);

            token.Should().NotBeNullOrEmpty();
            token.Split('.').Should().HaveCount(3);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Issuer.Should().Be("orcamentaria.auth");
            jwt.Audiences.Should().Contain("orcamentaria.user");
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(user.Id.ToString());
            jwt.Claims.First(c => c.Type == "TokenType").Value.Should().Be("RefreshToken");
            jwt.Claims.First(c => c.Type == "token_use").Value.Should().Be("user");
        }

        [Fact]
        public void Generate_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var user = _fixture.CreateEntity(1);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            Action act = () => _service.Generate(user);

            act.Should().Throw<UnexpectedException>();
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_WhenTokenGeneratedByGenerate_ReturnsUserId()
        {
            var rsa = RSA.Create(2048);
            var securityKey = new RsaSecurityKey(rsa);
            var user = _fixture.CreateEntity(1);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(securityKey);

            var token = _service.Generate(user);

            var result = await _service.ValidateAsync(token);

            result.Should().Be(user.Id);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenIsMalformed_ThrowsInfoException()
        {
            var rsa = RSA.Create(2048);
            var securityKey = new RsaSecurityKey(rsa);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(securityKey);

            Func<Task> act = async () => await _service.ValidateAsync("token-invalido");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.ValidateAsync("qualquer-token");

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
