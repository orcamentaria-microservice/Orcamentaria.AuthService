using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IBootstrapRepository
    {
        Bootstrap? GetById(long Id);
        Bootstrap? GetActiveByServiceId(long serviceId);
        Task<Bootstrap> Insert(Bootstrap bootstrap);
        Task<Bootstrap> UpdateHash(long id, string hash);
        Task<Bootstrap> Inactive(long id);
    }
}
