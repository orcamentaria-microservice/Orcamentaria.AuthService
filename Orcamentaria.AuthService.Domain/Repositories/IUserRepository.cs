using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Repositories;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IUserRepository : IBasicRepository<User>
    {
        User? GetByEmail(string email);
        Task<User> UpdatePasswordAsync(long id, string password);
        Task<User> AddPermissionsAsync(long userId, IEnumerable<Permission> permissions);
        Task<User> RemovePermissionsAsync(long userId, IEnumerable<Permission> permissions);
    }
}
