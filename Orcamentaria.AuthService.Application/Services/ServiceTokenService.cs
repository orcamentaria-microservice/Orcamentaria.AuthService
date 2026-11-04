using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orcamentaria.AuthService.Application.Services
{
    public class ServiceTokenService : ITokenService<Service>
    {
        private readonly IPrivateKeyReader _keyReader;

        public ServiceTokenService(IPrivateKeyReader keyReader)
        {
            _keyReader = keyReader;
        }

        public string Generate(Service data)
        {
            try
            {
                var rsa = _keyReader.ReadServiceKey();

                rsa.KeyId = "auth-service-key-2";

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, data.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, data.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("token_use", "service")
                ];

                var token = new JwtSecurityToken(
                    issuer: "orcamentaria.auth",
                    audience: "orcamentaria.service",
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(12),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Task<long> ValidateAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
