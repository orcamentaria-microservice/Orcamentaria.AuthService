using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;

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
        public User GetById(long id)
            => _dbContext.Users
                .Include(x => x.Permissions)
                .Where(x => x.Id == id && x.CompanyId == _userAuthContext.UserCompanyId)
                .FirstOrDefault();

        public IEnumerable<User> GetByCompanyId()
            => _dbContext.Users.Where(x => x.CompanyId == _userAuthContext.UserCompanyId);

        public User GetByCredentials(string email, string password)
            => _dbContext.Users
            .Include(x => x.Permissions)
            .Where(x => x.Email == email && x.Password == password && x.CompanyId == _userAuthContext.UserCompanyId)
            .FirstOrDefault();

        public User GetByEmail(string email)
            => _dbContext.Users
                .Include(x => x.Permissions)
                .Where(x => x.Email == email && x.CompanyId == _userAuthContext.UserCompanyId)
                .FirstOrDefault();

        public async Task<User> Insert(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> Update(long id, User user)
        {
            var entity = _dbContext.Users.FirstOrDefault(p => p.Id == id && p.CompanyId == _userAuthContext.UserCompanyId);

            if (entity is not null)
            {
                entity.Name = user.Name;
                entity.Active = user.Active;
                entity.UpdateAt = user.UpdateAt;

                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<User> UpdatePassword(long id, string password)
        {
            var entity = _dbContext.Users.FirstOrDefault(p => p.Id == id);

            if (entity is not null)
            {
                entity.Password = password;

                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<User> AddPermission(long userId, long permissionId)
        {
            var userEntity = _dbContext.Users.Include(x => x.Permissions)
                .Where(x => x.Id == userId).FirstOrDefault();

            var permissionEntity = _dbContext.Permissions
                .Where(x => x.Id == permissionId).FirstOrDefault();

            userEntity.Permissions.Add(permissionEntity);

            await _dbContext.SaveChangesAsync();

            return userEntity;
        }

        public async Task<User> RemovePermission(long userId, long permissionId)
        {
            var userEntity = _dbContext.Users.Include(x => x.Permissions)
                .Where(x => x.Id == userId).FirstOrDefault();

            var permissionEntity = _dbContext.Permissions
                .Where(x => x.Id == permissionId).FirstOrDefault();

            userEntity.Permissions.Remove(permissionEntity);

            await _dbContext.SaveChangesAsync();

            return userEntity;
        }

    }
}
