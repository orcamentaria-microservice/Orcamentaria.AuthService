using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Orcamentaria.AuthService.Application.Services
{

    public class TokenService : ITokenService
    {
        private readonly string _private_key_service = "private_key_service.pem";
        private readonly string _private_key_user = "private_key_user.pem";
        private readonly string _public_key_user = "public_key_user.pem";
        private readonly string projectName = "Orcamentaria.AuthService.API";
        
        private readonly IRsaService _rsaService;

        public TokenService(IRsaService rsaService)
        {
            _rsaService = rsaService;
        }

        public Dictionary<string, string> GenerateSecrets(Service service)
        {
            try
            {
                byte[] key = new byte[32 + service.Id];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }
                var clientSecret = Convert.ToBase64String(key);

                var clientId = Guid.NewGuid().ToString();

                return new Dictionary<string, string> {
                    { "clientId", clientId },
                    { "clientSecret", clientSecret }
                };
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public string GenerateTokenService(Service service)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key_service);

                rsa.KeyId = "auth-service-key-1";

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, service.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, service.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

        public string GenerateTokenUser(User user)
        {
            try {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key_user);

                rsa.KeyId = "auth-user-key-1";

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Company", user.CompanyId.ToString()),
                    new Claim("TokenType", "Token"),
                    new Claim(ClaimTypes.Role, Guid.NewGuid().ToString()),
                    .. ConvertPermissionsToClaims(user.Permissions),
                ];

                var token = new JwtSecurityToken(
                    issuer: "orcamentaria.auth",
                    audience: "orcamentaria.user",
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(5),
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

        public string GenerateRefreshTokenUser(User user)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key_user);

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("TokenType", "RefreshToken"),
                ];

                var token = new JwtSecurityToken(
                    issuer: "orcamentaria.auth",
                    audience: "orcamentaria.user",
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

        public async Task<long> ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var rsa = _rsaService.GenerateRsaSecurityKey(projectName, _private_key_user);

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

                var tokenResult = await tokenHandler.ValidateTokenAsync(refreshToken, validationParameters);

                if (!tokenResult.IsValid)
                    throw new UnauthorizedException("Token inválido");

                var userIdClaim = tokenResult.Claims.FirstOrDefault(c => c.Key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var tokenType = tokenResult.Claims.FirstOrDefault(c => c.Key == "TokenType").Value;

                if (!tokenType.Equals("RefreshToken"))
                    throw new UnauthorizedAccessException("Token inválido.");

                if (userIdClaim is null)
                    throw new UnauthorizedAccessException("Token inválido.");

                if(!long.TryParse(userIdClaim.ToString(), out var userId))
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

        private IEnumerable<Claim> ConvertPermissionsToClaims(IEnumerable<Permission> permissions)
        {
            try 
	        {
                return permissions
                    .Select(x =>
                    {
                        var incrementalPermission = String.Empty;
                        if (!String.IsNullOrEmpty(x.IncrementalPermission))
                            incrementalPermission = $":{x.IncrementalPermission.ToUpper()}";

                        return new Claim(ClaimTypes.Role, $"{x.Resource.ToString().ToUpper()}:{x.Type.ToString().ToUpper()}{incrementalPermission}");
                    });
            }
	        catch (Exception ex)
	        {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
    }
}
