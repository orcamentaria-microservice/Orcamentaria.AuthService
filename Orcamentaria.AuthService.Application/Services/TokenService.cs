using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
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

        public string GenerateTokenService(Service service)
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

        public string GenerateTokenUser(User user)
        {
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

        public string GenerateRefreshTokenUser(User user)
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

        public long ValidateRefreshToken(string refreshToken)
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

            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                
                if (tokenType != "RefreshToken")
                {
                    return 0;
                }

                if (userIdClaim != null)
                {
                    return 0;
                }

                long.TryParse(userIdClaim.Value, out var userId);
                
                return userId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private string GetKey(string filename)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Orcamentaria.AuthService.API");

            var resourceName = assembly.GetManifestResourceNames()
                                       .FirstOrDefault(name => name.EndsWith(filename));

            if (resourceName is null)
                throw new Exception($"Recurso {filename} não encontrado.");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private IEnumerable<Claim> ConvertPermissionsToClaims(IEnumerable<Permission> permissions)
            => permissions
            .Select(x =>
            {
                var incrementalPermission = String.Empty;
                if(!String.IsNullOrEmpty(x.IncrementalPermission))
                    incrementalPermission = $":{x.IncrementalPermission.ToUpper()}";

                return new Claim(ClaimTypes.Role, $"{x.Resource.ToString().ToUpper()}:{x.Type.ToString().ToUpper()}{incrementalPermission}");
            });
    }
}
