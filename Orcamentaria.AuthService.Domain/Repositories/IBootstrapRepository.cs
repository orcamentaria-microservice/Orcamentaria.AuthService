using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Repositories;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IBootstrapRepository : IBasicRepository<Bootstrap>
    {
        Task<Bootstrap> UpdateHash(long id, string hash);
        Task<Bootstrap> Inactive(long id);
    }
}
