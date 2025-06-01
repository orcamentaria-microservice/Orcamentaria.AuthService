using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface ITokenService
    {
        string GenerateTokenUser(User user);
        string GenerateRefreshTokenUser(User user);
        string GenerateTokenService(Service service);
        Dictionary<string, string> GenerateSecrets(Service service);
        long ValidateRefreshToken(string refreshToken);
    }
}
