using Microsoft.AspNetCore.Mvc;
using Orcamentaria.APIGetaway.Domain.DTOs.Authentication;
using Orcamentaria.AuthService.Domain.DTOs.User;
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

        [HttpPost("Service/Authenticate/{clientId}/{clientSecret}", Name = "AuthenticateService")]
        public Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret)
        {
            try
            {
                //Reset Path
                HttpContext.Request.Path = "/api/v1/Authentication/Service/Authenticate";

                return _service.AuthenticateService(clientId, clientSecret);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("Bootstrap/Authenticate/{bootstrapSecret}", Name = "AuthenticateBootstrap")]
        public async Task<Response<AuthenticationServiceResponseDTO>> AuthenticateBootstrap(string bootstrapSecret)
        {
            try
            {
                //Reset Path
                HttpContext.Request.Path = "/api/v1/Authentication/Service/Authenticate/Bootstrap";

                return await _service.AuthenticateWithBootstrapSecret(bootstrapSecret);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("User/Authenticate/{email}/{password}", Name = "AuthenticateUser")]
        public Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password)
        {
            try
            {
                //Reset Path
                HttpContext.Request.Path = "/api/v1/Authentication/User/Authenticate";

                return _service.AuthenticateUser(email, password);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("User/RefreshToken", Name = "AuthenticateRefreshTokenUser")]
        public async Task<Response<AuthenticationUserResponseDTO>> RefreshTokenUser([FromBody] UserRefreshToken dto)
        {
            try
            {
                //Reset Path
                HttpContext.Request.Path = "/api/v1/Authentication/User/RefreshToken";

                return await _service.RefreshTokenUser(dto.RefreshToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
