using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MySqlContext _dbContext;
        private readonly IUserAuthContext _userAuthContext;

        public UserRepository(
            MySqlContext dbContext,
            IUserAuthContext userAuthContext)
        {
            _dbContext = dbContext;
            _userAuthContext = userAuthContext;
        }

        public User? GetById(long id)
        {
            try
            {
                return _dbContext.Users
                    .Include(x => x.Permissions)
                    .FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public IEnumerable<User> GetByCompanyId()
        {
            try
            {
                return _dbContext.Users.Where(x => x.CompanyId == _userAuthContext.UserCompanyId);
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

        public async Task<User> Insert(User user)
        {
            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> Update(long id, User user)
        {
            try
            {
                var entity = _dbContext.Users.First(
                    x => x.Id == id && x.CompanyId == _userAuthContext.UserCompanyId);

                entity.Name = user.Name;
                entity.Active = user.Active;
                entity.UpdateAt = user.UpdateAt;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> UpdatePassword(long id, string password)
        {
            try
            {
                var entity = _dbContext.Users.First(p => p.Id == id);
            
                entity.Password = password;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> AddPermissions(long userId, IEnumerable<Permission> permissions)
        {
            try
            {
                var userEntity = _dbContext.Users
                    .Include(x => x.Permissions)
                    .First(x => x.Id == userId);

                userEntity.Permissions = userEntity.Permissions.Union(permissions).ToList();

                await _dbContext.SaveChangesAsync();

                return userEntity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<User> RemovePermissions(long userId, IEnumerable<Permission> permissions)
        {
            try
            {
                var userEntity = _dbContext.Users
                    .Include(x => x.Permissions)
                    .First(x => x.Id == userId);

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
    }
}
