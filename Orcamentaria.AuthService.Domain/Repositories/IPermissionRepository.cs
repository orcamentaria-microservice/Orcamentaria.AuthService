using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Repositories
{
    public interface IPermissionRepository
    {
        Permission? GetById(long id);
        IEnumerable<Permission> GetByResource(ResourceEnum resource);
        IEnumerable<Permission> GetByType(PermissionTypeEnum type);
        Task<Permission> Insert(Permission permission);
        Task<Permission> Update(long id, Permission permission);
    }
}
