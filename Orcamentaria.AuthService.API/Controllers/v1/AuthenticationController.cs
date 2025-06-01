using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.Authentication;
using Orcamentaria.AuthService.Domain.DTOs.Authentication;
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

        [HttpPost("Service/Authenticate", Name = "ServiceAuthenticate")]
        public Response<AuthenticationServiceResponseDTO> AuthenticateService(
            [FromHeader] string clientId, [FromHeader] string clientSecret)
            => _service.AuthenticateService(clientId, clientSecret);

        [HttpPost("User/Authenticate", Name = "UserAuthenticate")]
        public Response<AuthenticationUserResponseDTO> AuthenticateUser([FromBody] dynamic dto)
            => _service.AuthenticateUser(dto.clientId, dto.clientSecret);

        [HttpPost("User/RefreshToken/{refreshToken}", Name = "UserRefreshToken")]
        public Response<AuthenticationUserResponseDTO> RefreshTokenUser(string refreshToken)
            => _service.RefreshTokenUser(refreshToken);

    }
}
