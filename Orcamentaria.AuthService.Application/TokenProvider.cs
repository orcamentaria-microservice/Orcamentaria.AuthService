using Microsoft.Extensions.Options;
using Orcamentaria.AuthService.Domain;
using Orcamentaria.AuthService.Domain.Services;

namespace Orcamentaria.AuthService.Application
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IOptions<AuthenticationSecretsOptions> _options;

        public TokenProvider(
            IAuthenticationService authenticationService,
            IOptions<AuthenticationSecretsOptions> options
            )
        {
            _authenticationService = authenticationService;
            _options = options;
        }

        public async Task<string> GetTokenAsync()
        {
            var result = _authenticationService.AuthenticateService(
                _options.Value.ClientId, _options.Value.ClientSecret);

            return result.Data.Token;
        }
    }
}
