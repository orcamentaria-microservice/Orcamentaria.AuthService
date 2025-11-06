using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class BootstrapRepository : BasicRepository<Bootstrap>, IBootstrapRepository
    {

        private readonly MySqlContext _dbContext;
        private readonly IUserAuthContext _userAuthContext;

        public BootstrapRepository(
            MySqlContext dbContext, 
            IUserAuthContext userAuthContext) : base(dbContext, userAuthContext)
        {
            _dbContext = dbContext;
            _userAuthContext = userAuthContext;
        }

        public async Task<Bootstrap> Inactive(long id)
        {
            try
            {
                var entity = _dbContext.Bootstraps.FirstOrDefault(p => p.Id == id);

                entity.Active = false;
                entity.RevokedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Bootstrap> UpdateHash(long id, string hash)
        {
            try
            {
                var entity = _dbContext.Bootstraps.First(p => p.Id == id);

                entity.Hash = hash;

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
