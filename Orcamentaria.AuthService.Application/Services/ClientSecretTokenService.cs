using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Exceptions;
using System.Security.Cryptography;

namespace Orcamentaria.AuthService.Application.Services
{
    public class ClientSecretTokenService : ITokenService<Service>
    {
        public string Generate(Service data)
        {
            try
            {
                byte[] key = new byte[32 + data.Id];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }

                var clientSecret = Convert.ToBase64String(key);

                return clientSecret;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Task<long> Validate(string token)
        {
            throw new NotImplementedException();
        }
    }
}
