using Microsoft.Extensions.Options;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models.Configurations;
using Orcamentaria.Lib.Domain.Providers;

namespace Orcamentaria.AuthService.Application
{
    public class TokenProviderAuth : ITokenProvider
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IOptions<AuthenticationSecretsConfigurations> _authenticationServiceConfiguration;

        public TokenProviderAuth(
            IAuthenticationService authenticationService,
            IOptions<AuthenticationSecretsConfigurations> authenticationServiceConfiguration
            )
        {
            _authenticationService = authenticationService;
            _authenticationServiceConfiguration = authenticationServiceConfiguration;
        }

        public async Task<string> GetTokenAsync()
        {
            var result = _authenticationService.AuthenticateService(
                _authenticationServiceConfiguration.Value.ClientId, 
                _authenticationServiceConfiguration.Value.ClientSecret);

            return result.Data.Token;
        }
    }
}
