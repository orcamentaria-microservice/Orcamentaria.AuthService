using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly MySqlContext _dbContext;

        public PermissionRepository(MySqlContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Permission? GetById(long id)
        {
            try
            {
                return _dbContext.Permissions.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public IEnumerable<Permission> GetByResource(ResourceEnum resource)
        {
            try
            {
                return _dbContext.Permissions.Where(x => x.Resource == resource);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public IEnumerable<Permission> GetByType(PermissionTypeEnum type)
        {
            try
            {
                return _dbContext.Permissions.Where(x => x.Type == type);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Permission> Insert(Permission permission)
        {
            try
            {
                _dbContext.Permissions.Add(permission);
                await _dbContext.SaveChangesAsync();
                return permission;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Permission> Update(long id, Permission permission)
        {
            try
            {
                var entity = _dbContext.Permissions.FirstOrDefault(p => p.Id == id);

                entity.Resource = permission.Resource;
                entity.Description = permission.Description;
                entity.Type = permission.Type;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
    }
}
