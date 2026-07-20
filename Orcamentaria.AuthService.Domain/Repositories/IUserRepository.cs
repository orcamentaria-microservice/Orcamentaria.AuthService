using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using System.Linq.Expressions;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IUserRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(long id, params Expression<Func<TEntity, object>>[] includes);
        Task<(IEnumerable<TEntity?>, ResponsePagination pagination)> GetAsync(GridParams gridParams, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity> UpdateAsync(long id, TEntity entity);
        TEntity? GetByEmail(string email);
        Task<TEntity> UpdatePasswordAsync(long id, string password);
        Task<TEntity> AddPermissionsAsync(long userId, IEnumerable<Permission> permissions);
        Task<TEntity> RemovePermissionsAsync(long userId, IEnumerable<Permission> permissions);
    }
}
