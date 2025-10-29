using Orcamentaria.Lib.Domain.DTOs.Authentication;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IAuthenticationService
    {
        Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password);
        Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret);
        Task<Response<AuthenticationUserResponseDTO>> RefreshTokenUser(string refreshToken);
        Task<Response<AuthenticationServiceResponseDTO>> AuthenticateWithBootstrapSecret(string bootstrapSecret);
        
    }
}
