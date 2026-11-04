using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Integration
{
    [Collection(nameof(UserCollection))]
    public sealed class TokenIntegrationTest : IDisposable
    {
        private readonly UserFixture _fixture;
        private readonly string _tempDir;

        public TokenIntegrationTest(UserFixture fixture)
        {
            _fixture = fixture;
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private (string privatePath, RsaSecurityKey publicKey) GenerateTemporaryPair(string fileName)
        {
            using var rsa = RSA.Create(2048);
            var privatePem = rsa.ExportRSAPrivateKeyPem();
            var path = Path.Combine(_tempDir, fileName);
            File.WriteAllText(path, privatePem);

            // Public key for validation
            var publicRsa = RSA.Create();
            publicRsa.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
            return (path, new RsaSecurityKey(publicRsa));
        }

        private static IPrivateKeyReader BuildReader(string userPath, string servicePath)
        {
            var values = new Dictionary<string, string?>
            {
                ["Auth:Keys:UserPrivateKeyPath"] = userPath,
                ["Auth:Keys:ServicePrivateKeyPath"] = servicePath
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            return new PrivateKeyReader(config);
        }

        #region User token

        [Fact]
        public void UserToken_WhenSignedWithNewKey_ValidatesWithMatchingPublicKey()
        {
            var (userPrivatePath, userPublicKey) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, _) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var user = _fixture.CreateEntity(1);
            user.Permissions = new List<Permission>();
            var service = new UserTokenService(reader);

            var token = service.Generate(user);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "orcamentaria.auth",
                ValidateAudience = true,
                ValidAudience = "orcamentaria.user",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = userPublicKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);

            principal.Should().NotBeNull();
        }

        [Fact]
        public void UserToken_WhenValidatedWithWrongPublicKey_ThrowsSecurityTokenSignatureKeyNotFoundException()
        {
            var (userPrivatePath, _) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, _) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var user = _fixture.CreateEntity(1);
            user.Permissions = new List<Permission>();
            var service = new UserTokenService(reader);

            var token = service.Generate(user);

            // Wrong key — a freshly generated RSA pair unrelated to the signing key
            using var wrongRsa = RSA.Create(2048);
            var wrongPublicKey = new RsaSecurityKey(wrongRsa);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "orcamentaria.auth",
                ValidateAudience = true,
                ValidAudience = "orcamentaria.user",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = wrongPublicKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var act = () => handler.ValidateToken(token, validationParameters, out _);

            act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
        }

        [Fact]
        public void UserToken_WhenValid_HasKeyIdAuthUserKey2()
        {
            var (userPrivatePath, _) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, _) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var user = _fixture.CreateEntity(1);
            user.Permissions = new List<Permission>();
            var service = new UserTokenService(reader);

            var token = service.Generate(user);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Header.Kid.Should().Be("auth-user-key-2");
        }

        #endregion

        #region Service token

        [Fact]
        public void ServiceToken_WhenSignedWithNewKey_ValidatesWithMatchingPublicKey()
        {
            var (userPrivatePath, _) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, servicePublicKey) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var serviceEntity = new Service { Id = 1, Name = "TestService" };
            var service = new ServiceTokenService(reader);

            var token = service.Generate(serviceEntity);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "orcamentaria.auth",
                ValidateAudience = true,
                ValidAudience = "orcamentaria.service",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = servicePublicKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);

            principal.Should().NotBeNull();
        }

        [Fact]
        public void ServiceToken_WhenValidatedWithWrongPublicKey_ThrowsSecurityTokenSignatureKeyNotFoundException()
        {
            var (userPrivatePath, _) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, _) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var serviceEntity = new Service { Id = 1, Name = "TestService" };
            var service = new ServiceTokenService(reader);

            var token = service.Generate(serviceEntity);

            using var wrongRsa = RSA.Create(2048);
            var wrongPublicKey = new RsaSecurityKey(wrongRsa);

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "orcamentaria.auth",
                ValidateAudience = true,
                ValidAudience = "orcamentaria.service",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = wrongPublicKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var act = () => handler.ValidateToken(token, validationParameters, out _);

            act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
        }

        [Fact]
        public void ServiceToken_WhenValid_HasKeyIdAuthServiceKey2()
        {
            var (userPrivatePath, _) = GenerateTemporaryPair("user-signing-key.pem");
            var (servicePrivatePath, _) = GenerateTemporaryPair("service-signing-key.pem");
            var reader = BuildReader(userPrivatePath, servicePrivatePath);

            var serviceEntity = new Service { Id = 1, Name = "TestService" };
            var service = new ServiceTokenService(reader);

            var token = service.Generate(serviceEntity);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Header.Kid.Should().Be("auth-service-key-2");
        }

        #endregion
    }
}
