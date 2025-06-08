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

        [HttpPost("Service/Authenticate/{clientId}/{clientSecret}", Name = "ServiceAuthenticate")]
        public Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret)
        {
            try
            {
                //Reset Path
                return _service.AuthenticateService(clientId, clientSecret);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("User/Authenticate/{email}/{password}", Name = "UserAuthenticate")]
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

        [HttpPost("User/RefreshToken", Name = "UserRefreshToken")]
        public Response<AuthenticationUserResponseDTO> RefreshTokenUser([FromBody] UserRefreshToken dto)
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
