using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.Security.Cryptography;
using Xunit;

namespace Orcamentaria.AuthService.Test.Keys
{
    public sealed class PrivateKeyReaderTest : IDisposable
    {
        private readonly string _tempDir;

        public PrivateKeyReaderTest()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private static string WriteKeyToFile(string directory, string fileName)
        {
            using var rsa = RSA.Create(2048);
            var pem = rsa.ExportRSAPrivateKeyPem();
            var path = Path.Combine(directory, fileName);
            File.WriteAllText(path, pem);
            return path;
        }

        private static IConfiguration BuildConfiguration(string userPath, string servicePath)
        {
            var values = new Dictionary<string, string?>
            {
                ["Auth:Keys:UserPrivateKeyPath"] = userPath,
                ["Auth:Keys:ServicePrivateKeyPath"] = servicePath
            };
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        #region Constructor — valid paths

        [Fact]
        public void Constructor_WhenBothPathsValid_LoadsRsaKeySuccessfully()
        {
            var userPath = WriteKeyToFile(_tempDir, "user-signing-key.pem");
            var servicePath = WriteKeyToFile(_tempDir, "service-signing-key.pem");
            var config = BuildConfiguration(userPath, servicePath);

            var reader = new PrivateKeyReader(config);

            reader.ReadUserKey().Should().BeOfType<RsaSecurityKey>();
            reader.ReadServiceKey().Should().BeOfType<RsaSecurityKey>();
        }

        [Fact]
        public void ReadUserKey_WhenValidPath_ReturnsKeyWithRsaParameters()
        {
            var userPath = WriteKeyToFile(_tempDir, "user-signing-key.pem");
            var servicePath = WriteKeyToFile(_tempDir, "service-signing-key.pem");
            var config = BuildConfiguration(userPath, servicePath);

            var reader = new PrivateKeyReader(config);
            var key = reader.ReadUserKey();

            key.Rsa.Should().NotBeNull();
            key.Rsa!.KeySize.Should().Be(2048);
        }

        #endregion

        #region Constructor — missing paths

        [Fact]
        public void Constructor_WhenUserKeyPathMissing_ThrowsConfigurationException()
        {
            var servicePath = WriteKeyToFile(_tempDir, "service-signing-key.pem");
            var config = BuildConfiguration(
                Path.Combine(_tempDir, "nonexistent-user.pem"),
                servicePath);

            var act = () => new PrivateKeyReader(config);

            act.Should().Throw<ConfigurationException>()
               .WithMessage("*UserPrivateKeyPath*");
        }

        [Fact]
        public void Constructor_WhenServiceKeyPathMissing_ThrowsConfigurationException()
        {
            var userPath = WriteKeyToFile(_tempDir, "user-signing-key.pem");
            var config = BuildConfiguration(
                userPath,
                Path.Combine(_tempDir, "nonexistent-service.pem"));

            var act = () => new PrivateKeyReader(config);

            act.Should().Throw<ConfigurationException>()
               .WithMessage("*ServicePrivateKeyPath*");
        }

        [Fact]
        public void Constructor_WhenUserKeyPathEmpty_ThrowsConfigurationException()
        {
            var servicePath = WriteKeyToFile(_tempDir, "service-signing-key.pem");
            var config = BuildConfiguration(string.Empty, servicePath);

            var act = () => new PrivateKeyReader(config);

            act.Should().Throw<ConfigurationException>()
               .WithMessage("*UserPrivateKeyPath*");
        }

        #endregion

        #region Constructor — no key content in exception message

        [Fact]
        public void Constructor_WhenKeyPathMissing_ExceptionMessageDoesNotExposeKeyContent()
        {
            var config = BuildConfiguration(
                Path.Combine(_tempDir, "nonexistent.pem"),
                Path.Combine(_tempDir, "nonexistent.pem"));

            var act = () => new PrivateKeyReader(config);

            var exception = act.Should().Throw<ConfigurationException>().Which;
            exception.Message.Should().NotContain("BEGIN RSA PRIVATE KEY");
            exception.Message.Should().NotContain("END RSA PRIVATE KEY");
        }

        #endregion
    }
}
