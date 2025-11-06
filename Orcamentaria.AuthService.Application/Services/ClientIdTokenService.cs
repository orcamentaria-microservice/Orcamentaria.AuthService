using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;

namespace Orcamentaria.AuthService.Application.Services
{
    public class ClientIdTokenService : ITokenService<Service>
    {
        public string Generate(Service data)
        {
            try
            {
                var clientId = Guid.NewGuid().ToString();

                return clientId;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Task<long> ValidateAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
