using Microsoft.Extensions.Options;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models.Configurations;
using Orcamentaria.Lib.Domain.Providers;

namespace Orcamentaria.AuthService.Application.Providers
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IOptions<ServiceConfiguration> _serviceConfiguration;

        public TokenProvider(
            IAuthenticationService authenticationService,
            IOptions<ServiceConfiguration> serviceConfiguration
            )
        {
            _authenticationService = authenticationService;
            _serviceConfiguration = serviceConfiguration;
        }

        public async Task<string> GetTokenServiceAsync()
        {
            var result = _authenticationService.AuthenticateService(
                _serviceConfiguration.Value.ClientId,
                _serviceConfiguration.Value.ClientSecret);

            return result.Data.Token;
        }
    }
}
