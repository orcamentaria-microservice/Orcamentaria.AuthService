using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly MySqlContext _dbContext;

        public PermissionRepository(MySqlContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Permission GetById(long id)
            => _dbContext.Permissions.Where(x => x.Id == id).FirstOrDefault();

        public IEnumerable<Permission> GetByResource(ResourceEnum resource)
            => _dbContext.Permissions
            .Where(x => x.Resource == resource);

        public IEnumerable<Permission> GetByType(PermissionTypeEnum type)
            => _dbContext.Permissions.Where(x => x.Type == type);

        public async Task<Permission> Insert(Permission permission)
        {
            _dbContext.Permissions.Add(permission);
            await _dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission> Update(long id, Permission permission)
        {
            var entity = _dbContext.Permissions.FirstOrDefault(p => p.Id == id);

            if (entity is not null)
            {
                entity.Resource = permission.Resource;
                entity.Description = permission.Description;
                entity.Type = permission.Type;

                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }
    }
}
