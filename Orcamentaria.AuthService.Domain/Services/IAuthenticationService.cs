using Orcamentaria.Lib.Domain.DTOs.Authentication;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IAuthenticationService
    {
        Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password);
        Task<Response<AuthenticationServiceResponseDTO>> AuthenticateServiceAsync(string clientId, string clientSecret);
        Task<Response<AuthenticationUserResponseDTO>> RefreshTokenUserAsync(string refreshToken);
        Task<Response<AuthenticationServiceResponseDTO>> AuthenticateWithBootstrapSecretAsync(string bootstrapSecret);
        
    }
}
