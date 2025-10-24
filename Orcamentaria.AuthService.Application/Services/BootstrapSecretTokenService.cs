using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.Security.Cryptography;

namespace Orcamentaria.AuthService.Application.Services
{
    public class BootstrapSecretTokenService : ITokenService<Bootstrap>
    {
        private readonly IBootstrapRepository _bootstrapRepository;

        public BootstrapSecretTokenService(IBootstrapRepository bootstrapRepository)
        {
            _bootstrapRepository = bootstrapRepository;
        }

        public string Generate(Bootstrap data)
        {
            var secretBytes = RandomNumberGenerator.GetBytes(32);
            var secretBase64 = Base64UrlEncode(secretBytes);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(secretBytes);
            var hash = Convert.ToHexString(hashBytes);

            var token = $"{data.Id}.{secretBase64}";

            return $"{token} || {hash}";
        }

        public Task<long> Validate(string token)
        {
            try
            {
                var parts = token.Split('.', 2);
                if (parts.Length != 2)
                    throw new UnauthorizedException("Token inválido");

                var id = long.Parse(parts[0]);
                var secret = parts[1];

                var bootstrap = _bootstrapRepository.GetById(id);
                if (bootstrap == null || !bootstrap.Active || bootstrap.ExpiresAt < DateTime.UtcNow)
                    throw new UnauthorizedException("Token inválido");

                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Base64UrlDecode(secret));
                var hashHex = Convert.ToHexString(hashBytes);

                if (!hashHex.Equals(bootstrap.Hash, StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedException("Token inválido");

                return Task.FromResult(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private byte[] Base64UrlDecode(string base64Url)
        {
            string padded = base64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
