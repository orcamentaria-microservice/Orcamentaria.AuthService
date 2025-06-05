using Microsoft.AspNetCore.Mvc;
using Orcamentaria.APIGetaway.Domain.DTOs.Authentication;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service)
        {
            _service = service;
        }

        [HttpPost("Service/Authenticate/{clientId}/{clientSecret}", Name = "ServiceAuthenticate")]
        public Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret)
            => _service.AuthenticateService(clientId, clientSecret);

        [HttpPost("User/Authenticate/{email}/{password}", Name = "UserAuthenticate")]
        public Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password)
            => _service.AuthenticateUser(email, password);

        [HttpPost("User/RefreshToken/{refreshToken}", Name = "UserRefreshToken")]
        public Response<AuthenticationUserResponseDTO> RefreshTokenUser(string refreshToken)
            => _service.RefreshTokenUser(refreshToken);

    }
}
