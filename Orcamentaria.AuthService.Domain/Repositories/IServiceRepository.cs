using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IServiceRepository
    {
        Service? GetById(long id);
        Service? GetByCredentials(string clientId, string clientSecret);
        Task<Service> Insert(Service service);
        Task<Service> Update(long id, Service address);
        Task<Service> UpdateCredentials(long id, string clientId, string clientSecret);
    }
}

