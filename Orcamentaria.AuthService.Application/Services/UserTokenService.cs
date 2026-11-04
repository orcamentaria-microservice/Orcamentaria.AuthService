using Microsoft.IdentityModel.Tokens;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Orcamentaria.AuthService.Application.Services
{

    public class UserTokenService : ITokenService<User>
    {
        private readonly IPrivateKeyReader _keyReader;

        public UserTokenService(IPrivateKeyReader keyReader)
        {
            _keyReader = keyReader;
        }

        public string Generate(User data)
        {
            try
            {
                var rsa = _keyReader.ReadUserKey();

                rsa.KeyId = "auth-user-key-2";

                var credentials = new SigningCredentials(rsa, SecurityAlgorithms.RsaSha256);

                List<Claim> claims =
                [
                    new Claim(JwtRegisteredClaimNames.Sub, data.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, data.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Company", data.CompanyId.ToString()),
                    new Claim("TokenType", "Token"),
                    new Claim("token_use", "user"),
                    new Claim(ClaimTypes.Role, Guid.NewGuid().ToString()),
                    .. ConvertPermissionsToClaims(data.Permissions),
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

        public Task<long> ValidateAsync(string token)
        {
            throw new NotImplementedException();
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

                        if (x.Resource == ResourceEnum.MASTER)
                            return new Claim(ClaimTypes.Role, x.Resource.ToString().ToUpper());

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
