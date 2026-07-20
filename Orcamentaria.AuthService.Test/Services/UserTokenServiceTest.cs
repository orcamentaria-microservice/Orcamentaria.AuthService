using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(UserCollection))]
    public class UserTokenServiceTest
    {
        private readonly UserFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.UserTokenService _service;

        public UserTokenServiceTest(UserFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.UserTokenService>(true);
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
        public void Generate_WhenUserHasNoPermissions_ReturnsTokenWithExpectedClaims()
        {
            SetupValidRsaService();
            var data = _fixture.CreateEntity(1);
            data.Permissions = new List<Permission>();

            var token = _service.Generate(data);

            token.Should().NotBeNullOrEmpty();
            token.Split('.').Should().HaveCount(3);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Issuer.Should().Be("orcamentaria.auth");
            jwt.Audiences.Should().Contain("orcamentaria.user");
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == data.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == data.Email);
            jwt.Claims.Should().Contain(c => c.Type == "Company" && c.Value == data.CompanyId.ToString());
            jwt.Claims.Should().Contain(c => c.Type == "TokenType" && c.Value == "Token");
            jwt.Claims.Should().Contain(c => c.Type == "token_use" && c.Value == "user");
        }

        [Fact]
        public void Generate_WhenUserHasNonMasterPermissions_ReturnsRoleClaimForEachPermission()
        {
            SetupValidRsaService();
            var data = _fixture.CreateEntity(1);
            var permissions = _fixture.CreatePermissions(2).ToList();
            permissions[0].IncrementalPermission = "special";
            data.Permissions = permissions;

            var token = _service.Generate(data);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            foreach (var permission in permissions)
            {
                var incrementalPermission = string.IsNullOrEmpty(permission.IncrementalPermission)
                    ? string.Empty
                    : $":{permission.IncrementalPermission.ToUpper()}";

                var expectedValue = $"{permission.Resource.ToString().ToUpper()}:{permission.Type.ToString().ToUpper()}{incrementalPermission}";

                jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == expectedValue);
            }

            jwt.Claims.Count(c => c.Type == ClaimTypes.Role).Should().Be(permissions.Count + 1);
        }

        [Fact]
        public void Generate_WhenUserHasMasterPermission_ReturnsRoleClaimEqualsMaster()
        {
            SetupValidRsaService();
            var data = _fixture.CreateEntity(1);
            data.Permissions = new List<Permission>
            {
                new Permission
                {
                    Id = 1,
                    Description = "MasterPermission",
                    Resource = ResourceEnum.MASTER,
                    Type = PermissionTypeEnum.READ,
                    IncrementalPermission = string.Empty
                }
            };

            var token = _service.Generate(data);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "MASTER");
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
