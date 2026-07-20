using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class BootstrapRepository : BaseRepository<Bootstrap>, IBootstrapRepository<Bootstrap>
    {

        private readonly MySqlContext _dbContext;

        public BootstrapRepository(
            MySqlContext dbContext, 
            IUserAuthContext userAuthContext) : base(dbContext, userAuthContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Bootstrap> Inactive(long id)
        {
            try
            {
                var entity = _dbContext.Bootstraps.FirstOrDefault(p => p.Id == id);

                if (entity is null)
                    throw new NotFoundException("Id não encontrado.");

                entity.Active = false;
                entity.RevokedAt = DateTime.Now;

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

        public async Task<Bootstrap> UpdateHash(long id, string hash)
        {
            try
            {
                var entity = _dbContext.Bootstraps.FirstOrDefault(p => p.Id == id);

                if (entity is null)
                    throw new NotFoundException("Id não encontrado.");

                entity.Hash = hash;

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
    }
}
