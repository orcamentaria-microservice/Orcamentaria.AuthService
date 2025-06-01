using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        User GetById(long id);
        User GetByCredentials(string email, string password);
        User GetByEmail(string email);
        IEnumerable<User> GetByCompanyId();
        Task<User> Insert(User user);
        Task<User> Update(long id, User user);
        Task<User> UpdatePassword(long id, string password);
        Task<User> AddPermission(long userId, long permissionId);
        Task<User> RemovePermission(long userId, long permissionId);
    }
}
