using Orcamentaria.AuthService.Domain.Authentication;
using Orcamentaria.AuthService.Domain.DTOs.Authentication;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IAuthenticationService
    {
        Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password);
        Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret);
        Response<AuthenticationUserResponseDTO> RefreshTokenUser(string refreshToken);
    }
}
