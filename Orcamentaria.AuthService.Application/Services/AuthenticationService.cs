using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.Lib.Domain.DTOs.Authentication;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;

namespace Orcamentaria.AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IServiceService _serviceService;
        private readonly IServiceProvider _provider;
        private readonly IPasswordService _passwordService;
        private readonly IBootstrapService _bootstrapService;

        public AuthenticationService(
            IUserService userService, 
            IServiceService serviceService,
            IServiceProvider provider,
            IPasswordService passwordService,
            IBootstrapService bootstrapService)
        {
            _userService = userService;
            _serviceService = serviceService;
            _provider = provider;
            _passwordService = passwordService;
            _bootstrapService = bootstrapService;
        }

        public Response<AuthenticationServiceResponseDTO> AuthenticateService(string clientId, string clientSecret)
        {
            try
            {
                var serviceTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("serviceToken");
                var service = _serviceService.GetByCredentials(clientId, clientSecret);

                if (service is null)
                    throw new InfoException($"Credenciais inválidas.", ErrorCodeEnum.NotFound);

                var token = serviceTokenService.Generate(service);

                return new Response<AuthenticationServiceResponseDTO>(new AuthenticationServiceResponseDTO
                {
                    ServiceId = service.Id,
                    Name = service.Name,
                    Token = token,
                });
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Response<AuthenticationUserResponseDTO> AuthenticateUser(string email, string password)
        {
            try
            {
                var userTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userToken");
                var userRefreshTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userRefreshToken");
                
                var user = _userService.GetUserByCredential(email);

                if (user is null)
                    throw new InfoException($"Email e/ou senha inválidos.", ErrorCodeEnum.NotFound);

                if(!_passwordService.PasswordIsValid(password, user.Password))
                    throw new InfoException($"Email e/ou senha inválidos.", ErrorCodeEnum.NotFound);

                var token = userTokenService.Generate(user);
                var refreshToken = userRefreshTokenService.Generate(user);

                return new Response<AuthenticationUserResponseDTO>(new AuthenticationUserResponseDTO
                {
                    UserId = user.Id,
                    CompanyId = user.CompanyId,
                    Name = user.Name,
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<AuthenticationUserResponseDTO>> RefreshTokenUser(string refreshToken)
        {
            try
            {
                var userTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userToken");
                var userRefreshTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userRefreshToken");

                var userId = await userRefreshTokenService.Validate(refreshToken);

                var user = _userService.GetById(userId);

                if (user is null)
                    throw new InfoException($"O {userId} não foi encontrado.", ErrorCodeEnum.NotFound);

                var token = userTokenService.Generate(user);
                var newRefreshToken = userRefreshTokenService.Generate(user);

                return new Response<AuthenticationUserResponseDTO>(new AuthenticationUserResponseDTO
                {
                    UserId = user.Id,
                    CompanyId = user.CompanyId,
                    Name = user.Name,
                    Token = token,
                    RefreshToken = newRefreshToken
                });
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<AuthenticationServiceResponseDTO>> AuthenticateWithBootstrapSecret(string bootstrapSecret)
        {
            try
            {
                var bootstrapSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Bootstrap>>("bootstrapSecretToken");
                var bootstrapTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("bootstrapToken");

                var bootstrapId = await bootstrapSecretTokenService.Validate(bootstrapSecret);

                var bootstrap = _bootstrapService.GetById(bootstrapId);
                if (!bootstrap.Success || bootstrap.Data is null)
                    throw new InfoException($"Ocorreu um erro na geração do token.", ErrorCodeEnum.NotFound);

                var service = _serviceService.GetById(bootstrap.Data.ServiceId);
                if (!service.Success || service.Data is null)
                    throw new InfoException($"Ocorreu um erro ao buscar serviço.", ErrorCodeEnum.NotFound);

                var token = bootstrapTokenService.Generate(new Service { Id = service.Data.Id, Name = service.Data.Name });

                var response = new AuthenticationServiceResponseDTO
                {
                    ServiceId = service.Data.Id,
                    Name = service.Data.Name,
                    Token = token,
                };

                return new Response<AuthenticationServiceResponseDTO>(response);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
    }
}
