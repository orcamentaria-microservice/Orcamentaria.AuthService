using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(ServiceCollection))]
    public class ServiceTokenServiceTest
    {
        private readonly ServiceFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.ServiceTokenService _service;

        public ServiceTokenServiceTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.ServiceTokenService>(true);
        }

        #region Generate

        [Fact]
        public void Generate_WhenValid_ReturnsSignedJwtToken()
        {
            var data = _fixture.CreateEntity(1);
            var rsa = RSA.Create(2048);
            var securityKey = new RsaSecurityKey(rsa);

            _mocker.GetMock<IPrivateKeyReader>()
                .Setup(r => r.ReadServiceKey())
                .Returns(securityKey);

            var token = _service.Generate(data);

            token.Should().NotBeNullOrWhiteSpace();
            token.Split('.').Should().HaveCount(3);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Issuer.Should().Be("orcamentaria.auth");
            jwt.Audiences.Should().Contain("orcamentaria.service");
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(data.Id.ToString());
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value.Should().Be(data.Name);
            jwt.Claims.First(c => c.Type == "token_use").Value.Should().Be("service");
            jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(12), TimeSpan.FromMinutes(5));

            _mocker.GetMock<IPrivateKeyReader>()
                .Verify(r => r.ReadServiceKey(), Times.Once());
        }

        [Fact]
        public void Generate_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var data = _fixture.CreateEntity(1);

            _mocker.GetMock<IPrivateKeyReader>()
                .Setup(r => r.ReadServiceKey())
                .Throws(new Exception());

            Action act = () => _service.Generate(data);

            act.Should().Throw<UnexpectedException>();
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_WhenCalled_ThrowsNotImplementedException()
        {
            Func<Task> act = async () => await _service.ValidateAsync("token");

            await act.Should().ThrowAsync<NotImplementedException>();
        }

        #endregion
    }
}
