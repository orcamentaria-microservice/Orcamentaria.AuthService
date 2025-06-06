using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
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
                var privateKey = GetKey(_private_key_service);
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());

                var credentials = new SigningCredentials(new RsaSecurityKey(rsa)
                {
                    KeyId = "auth-service-key-1"
                }, SecurityAlgorithms.RsaSha256);

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
                var privateKey = GetKey(_private_key_user);
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());

                var credentials = new SigningCredentials(new RsaSecurityKey(rsa)
                {
                    KeyId = "auth-user-key-1"
                }, SecurityAlgorithms.RsaSha256);

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
                var privateKey = GetKey(_private_key_user);
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());

                var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

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

        public long ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var publicKey = GetKey(_public_key_user);
                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey.ToCharArray());

                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "orcamentaria.auth",
                    ValidateAudience = true,
                    ValidAudience = "orcamentaria.user",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RequireExpirationTime = true,
                    ValidateTokenReplay = false
                };

                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;

                if (tokenType != "RefreshToken")
                    throw new UnauthorizedAccessException("Token inválido.");

                if (userIdClaim is null)
                    throw new UnauthorizedAccessException("Token inválido.");

                long.TryParse(userIdClaim.Value, out var userId);

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

        private string GetKey(string filename)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Orcamentaria.AuthService.API");

                if (assembly is null)
                    throw new ConfigurationException("Projeto 'Orcamentaria.AuthService.API' não encontrado.");

                var resourceName = assembly.GetManifestResourceNames()
                                           .FirstOrDefault(name => name.EndsWith(filename));

                if (resourceName is null)
                    throw new ConfigurationException("Faltando arquivo de configuração.");

                using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream is null)
                    throw new ConfigurationException("Faltando arquivo de configuração.");

                using var reader = new StreamReader(stream);

                return reader.ReadToEnd();
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
