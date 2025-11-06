using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.Lib.Domain.DTOs.Authentication;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;

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
                    throw new InfoException($"Credenciais invalidas.", ErrorCodeEnum.NotFound);

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
                
                var user = _userService.GetByEmail(email);

                if (user is null)
                    throw new InfoException($"Email e/ou senha invalidos.", ErrorCodeEnum.NotFound);

                if(!_passwordService.PasswordIsValid(password, user.Password))
                    throw new InfoException($"Email e/ou senha invalidos.", ErrorCodeEnum.NotFound);

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

        public async Task<Response<AuthenticationUserResponseDTO>> RefreshTokenUserAsync(string refreshToken)
        {
            try
            {
                var userTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userToken");
                var userRefreshTokenService = _provider.GetRequiredKeyedService<ITokenService<User>>("userRefreshToken");

                var userId = await userRefreshTokenService.ValidateAsync(refreshToken);

                var user = await _userService.GetByIdAsync(userId);

                if (user is null)
                    throw new InfoException($"O {userId} nao foi encontrado.", ErrorCodeEnum.NotFound);

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

        public async Task<Response<AuthenticationServiceResponseDTO>> AuthenticateWithBootstrapSecretAsync(string bootstrapSecret)
        {
            try
            {
                var bootstrapSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Bootstrap>>("bootstrapSecretToken");
                var bootstrapTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("bootstrapToken");

                var bootstrapId = await bootstrapSecretTokenService.ValidateAsync(bootstrapSecret);

                var bootstrap = await _bootstrapService.GetByIdAsync(bootstrapId);
                if (bootstrap is null)
                    throw new InfoException($"Ocorreu um erro na geracao do token.", ErrorCodeEnum.NotFound);

                var service = await _serviceService.GetByIdAsync(bootstrap.ServiceId);
                if (service is null)
                    throw new InfoException($"Ocorreu um erro ao buscar servico.", ErrorCodeEnum.NotFound);

                var token = bootstrapTokenService.Generate(new Service { Id = service.Id, Name = service.Name });

                var response = new AuthenticationServiceResponseDTO
                {
                    ServiceId = service.Id,
                    Name = service.Name,
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
