using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.Security.Cryptography;

namespace Orcamentaria.AuthService.Application.Services
{
    public sealed class PrivateKeyReader : IPrivateKeyReader
    {
        private readonly RsaSecurityKey _userKey;
        private readonly RsaSecurityKey _serviceKey;

        public PrivateKeyReader(IConfiguration configuration)
        {
            _userKey = LoadKey(configuration, "Auth:Keys:UserPrivateKeyPath", "UserPrivateKeyPath");
            _serviceKey = LoadKey(configuration, "Auth:Keys:ServicePrivateKeyPath", "ServicePrivateKeyPath");
        }

        public RsaSecurityKey ReadUserKey() => _userKey;
        public RsaSecurityKey ReadServiceKey() => _serviceKey;

        private static RsaSecurityKey LoadKey(IConfiguration configuration, string configKey, string keyName)
        {
            var filePath = configuration[configKey];

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new ConfigurationException($"Chave privada de assinatura não encontrada: {keyName}");

            try
            {
                var pem = File.ReadAllText(filePath);
                var rsa = RSA.Create();
                rsa.ImportFromPem(pem.ToCharArray());
                return new RsaSecurityKey(rsa);
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Chave privada de assinatura não encontrada: {keyName}", ex);
            }
        }
    }
}
