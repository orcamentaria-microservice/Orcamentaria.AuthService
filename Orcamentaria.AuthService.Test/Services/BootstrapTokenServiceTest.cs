using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(ServiceCollection))]
    public class BootstrapTokenServiceTest
    {
        private readonly ServiceFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.BootstrapTokenService _service;

        public BootstrapTokenServiceTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.BootstrapTokenService>(true);
        }

        private void SetupValidRsaService()
        {
            var rsa = RSA.Create(2048);
            var securityKey = new RsaSecurityKey(rsa);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(securityKey);
        }

        #region Generate

        [Fact]
        public void Generate_WhenValid_ReturnsJwtToken()
        {
            SetupValidRsaService();
            var data = _fixture.CreateEntity(1);

            var token = _service.Generate(data);

            token.Should().NotBeNullOrEmpty();
            token.Split('.').Should().HaveCount(3);
        }

        [Fact]
        public void Generate_WhenValid_ReturnsTokenWithExpectedClaims()
        {
            SetupValidRsaService();
            var data = _fixture.CreateEntity(1);

            var token = _service.Generate(data);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Issuer.Should().Be("orcamentaria.auth");
            jwt.Audiences.Should().Contain("orcamentaria.bootstrap");
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == data.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == data.Name);
            jwt.Claims.Should().Contain(c => c.Type == "token_use" && c.Value == "bootstrap");
            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "CONFIGURATION_BAG:READ");
        }

        [Fact]
        public void Generate_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var data = _fixture.CreateEntity(1);

            _mocker.GetMock<IRsaService>()
                .Setup(r => r.GenerateRsaSecurityKey(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());

            Action act = () => _service.Generate(data);

            act.Should().Throw<UnexpectedException>();
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_WhenCalled_ThrowsNotImplementedException()
        {
            Func<Task> act = () => _service.ValidateAsync("qualquer");

            await act.Should().ThrowAsync<NotImplementedException>();
        }

        #endregion
    }
}
