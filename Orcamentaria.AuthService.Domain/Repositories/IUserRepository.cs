using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        User GetById(long id);
        User GetByEmail(string email);
        IEnumerable<User> GetByCompanyId();
        Task<User> Insert(User user);
        Task<User> Update(long id, User user);
        Task<User> UpdatePassword(long id, string password);
        Task<User> AddPermissions(long userId, IEnumerable<Permission> permissions);
        Task<User> RemovePermissions(long userId, IEnumerable<Permission> permissions);
    }
}
