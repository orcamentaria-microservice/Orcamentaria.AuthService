using Orcamentaria.AuthService.Domain.Authentication;
using Orcamentaria.AuthService.Domain.DTOs.Authentication;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IServiceService _serviceService;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public AuthenticationService(
            IUserService userService, 
            IServiceService serviceService, 
            ITokenService tokenService, 
            IPasswordService passwordService)
        {
            _userService = userService;
            _serviceService = serviceService;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        public Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret)
        {
            try
            {
                var service = _serviceService.GetByCredentials(clientId, clientSecret);

                if (service is null)
                    return new Response<AuthenticationServiceResponseDTO>(ResponseErrorEnum.NotFound, "Credenciais inválidas.");

                var token = _tokenService.GenerateTokenService(service);

                return new Response<AuthenticationServiceResponseDTO>(new AuthenticationServiceResponseDTO
                {
                    ServiceId = service.Id,
                    Name = service.Name,
                    Token = token,
                });
            }
            catch (Exception ex)
            {
                return new Response<AuthenticationServiceResponseDTO>(ResponseErrorEnum.NotFound, "Credenciais inválidas.");
            }
        }

        public Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password)
        {
            try
            {
                var user = _userService.GetUserByCredential(email);

                if (user is null)
                    return new Response<AuthenticationUserResponseDTO>(ResponseErrorEnum.NotFound, "Email e/ou senha inválidos.");

                if(!_passwordService.PasswordIsValid(password, user.Password))
                    return new Response<AuthenticationUserResponseDTO>(ResponseErrorEnum.NotFound, "Email e/ou senha inválidos.");

                var token = _tokenService.GenerateTokenUser(user);
                var refreshToken = _tokenService.GenerateRefreshTokenUser(user);

                return new Response<AuthenticationUserResponseDTO>(new AuthenticationUserResponseDTO
                {
                    UserId = user.Id,
                    CompanyId = user.CompanyId,
                    Name = user.Name,
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return new Response<AuthenticationUserResponseDTO>(ResponseErrorEnum.NotFound, "Email e/ou senha inválidos.");
            }
        }

        public Response<AuthenticationUserResponseDTO> RefreshTokenUser(string refreshToken)
        {
            var userId = _tokenService.ValidateRefreshToken(refreshToken);

            if(userId == 0)
                return new Response<AuthenticationUserResponseDTO>(ResponseErrorEnum.NotFound, "Token inválido.");

            try
            {
                var user = _userService.GetById(userId);

                var token = _tokenService.GenerateTokenUser(user);
                var newRefreshToken = _tokenService.GenerateRefreshTokenUser(user);

                return new Response<AuthenticationUserResponseDTO>(new AuthenticationUserResponseDTO
                {
                    UserId = user.Id,
                    CompanyId = user.CompanyId,
                    Name = user.Name,
                    Token = token,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception)
            {
                return new Response<AuthenticationUserResponseDTO>(ResponseErrorEnum.NotFound, "Token inválido.");
            }
}
    }
}
