using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Repositories;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IServiceRepository : IBasicRepository<Service>
    {
        Service? GetByCredentials(string clientId, string clientSecret);
    }
}

