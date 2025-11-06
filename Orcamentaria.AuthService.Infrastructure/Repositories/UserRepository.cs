using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class UserRepository : BasicRepository<User>, IUserRepository
    {
        private readonly MySqlContext _dbContext;
        private readonly IUserAuthContext _userAuthContext;

        public UserRepository(
            MySqlContext dbContext,
            IUserAuthContext userAuthContext)
            : base(dbContext, userAuthContext)
        {
            _dbContext = dbContext;
            _userAuthContext = userAuthContext;
        }

        public async Task<User> UpdatePasswordAsync(long id, string password)
        {
            try
            {
                var entity = _dbContext.Users
                    .First(x => x.Id == id && x.CompanyId == _userAuthContext.CompanyId);
            
                entity.Password = password;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> AddPermissionsAsync(long userId, IEnumerable<Permission> permissions)
        {
            try
            {
                var userEntity = _dbContext.Users
                    .Include(x => x.Permissions)
                    .First(x => x.Id == userId && x.CompanyId == _userAuthContext.CompanyId);

                userEntity.Permissions = userEntity.Permissions.Union(permissions).ToList();

                await _dbContext.SaveChangesAsync();

                return userEntity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> RemovePermissionsAsync(long userId, IEnumerable<Permission> permissions)
        {
            try
            {
                var userEntity = _dbContext.Users
                    .Include(x => x.Permissions)
                    .First(x => x.Id == userId && x.CompanyId == _userAuthContext.CompanyId);

                foreach (var permission in permissions)
                {
                    userEntity.Permissions.Remove(permission);
                }

                await _dbContext.SaveChangesAsync();

                return userEntity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public User? GetByEmail(string email)
        {
            try
            {
                return _dbContext.Users
                    .Include(x => x.Permissions)
                    .FirstOrDefault(x => x.Email == email);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
    }
}
