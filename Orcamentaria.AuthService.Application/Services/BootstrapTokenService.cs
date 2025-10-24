using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orcamentaria.AuthService.Application.Services
{
    public class BootstrapTokenService : ITokenService<Service>
    {
        private readonly string _private_key = "private_key_service.pem";
        private readonly string projectName = "Orcamentaria.AuthService.API";

        private readonly IRsaService _rsaService;

        public BootstrapTokenService(IRsaService rsaService)
        {
            _rsaService = rsaService;
        }

        public string Generate(Service data)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key);

                rsa.KeyId = "auth-service-key-1";

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, data.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, data.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("token_use", "bootstrap"),
                    new Claim(ClaimTypes.Role, 
                        $"{ResourceEnum.CONFIGURATION_BAG.ToString().ToUpper()}:{PermissionTypeEnum.READ.ToString().ToUpper()}")
                ];

                var token = new JwtSecurityToken(
                    issuer: "orcamentaria.auth",
                    audience: "orcamentaria.bootstrap",
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(1),
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

        public Task<long> Validate(string token)
        {
            throw new NotImplementedException();
        }
    }
}
