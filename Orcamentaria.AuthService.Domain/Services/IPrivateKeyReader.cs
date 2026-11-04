using Microsoft.IdentityModel.Tokens;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IPrivateKeyReader
    {
        RsaSecurityKey ReadUserKey();
        RsaSecurityKey ReadServiceKey();
    }
}
