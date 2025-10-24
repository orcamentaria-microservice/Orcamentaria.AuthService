using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orcamentaria.AuthService.Application.Services
{
    public class UserRefreshTokenService : ITokenService<User>
    {
        private readonly string _private_key = "private_key_user.pem";
        private readonly string projectName = "Orcamentaria.AuthService.API";

        private readonly IRsaService _rsaService;

        public UserRefreshTokenService(IRsaService rsaService)
        {
            _rsaService = rsaService;
        }

        public string Generate(User data)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key);

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, data.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("TokenType", "RefreshToken"),
                    new Claim("token_use", "user")
                ];

                var token = new JwtSecurityToken(
                    issuer: "orcamentaria.auth",
                    audience: "orcamentaria",
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(10),
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

        public async Task<long> Validate(string token)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key);

                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "orcamentaria.auth",
                    ValidateAudience = true,
                    ValidAudience = "orcamentaria.user",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = rsa,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RequireExpirationTime = true,
                    ValidateTokenReplay = false
                };

                var tokenResult = await tokenHandler.ValidateTokenAsync(token, validationParameters);

                if (!tokenResult.IsValid)
                    throw new UnauthorizedException("Token inválido");

                var userIdClaim = tokenResult.Claims.FirstOrDefault(c => c.Key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var tokenType = tokenResult.Claims.FirstOrDefault(c => c.Key == "TokenType").Value;

                if (!tokenType.Equals("RefreshToken"))
                    throw new UnauthorizedAccessException("Token inválido.");

                if (userIdClaim is null)
                    throw new UnauthorizedAccessException("Token inválido.");

                if (!long.TryParse(userIdClaim.ToString(), out var userId))
                    throw new UnauthorizedAccessException("Token inválido.");

                return userId;
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
    }
}
