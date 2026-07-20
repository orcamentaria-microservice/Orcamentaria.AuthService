using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository<User>
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
                    .FirstOrDefault(x => x.Id == id && x.CompanyId == _userAuthContext.CompanyId);

                if (entity is null)
                    throw new NotFoundException("Usuário não encontrado.");

                entity.Password = password;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (DefaultException)
            {
                throw;
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
                    .FirstOrDefault(x => x.Id == userId && x.CompanyId == _userAuthContext.CompanyId);

                if (userEntity is null)
                    throw new NotFoundException("Usuário não encontrado.");

                userEntity.Permissions = userEntity.Permissions.Union(permissions).ToList();

                await _dbContext.SaveChangesAsync();

                return userEntity;
            }
            catch (DefaultException)
            {
                throw;
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
                    .FirstOrDefault(x => x.Id == userId && x.CompanyId == _userAuthContext.CompanyId);

                if (userEntity is null)
                    throw new NotFoundException("Usuário não encontrado.");

                foreach (var permission in permissions)
                {
                    userEntity.Permissions.Remove(permission);
                }

                await _dbContext.SaveChangesAsync();

                return userEntity;
            }
            catch (DefaultException)
            {
                throw;
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
