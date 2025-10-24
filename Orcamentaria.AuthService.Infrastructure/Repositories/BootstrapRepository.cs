using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class BootstrapRepository : IBootstrapRepository
    {
        private readonly MySqlContext _dbContext;

        public BootstrapRepository(MySqlContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Bootstrap? GetById(long Id)
        {
            try
            {
                return _dbContext.Bootstraps.FirstOrDefault(x => x.Id == Id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public Bootstrap? GetActiveByServiceId(long serviceId)
        {
            try
            {
                return _dbContext.Bootstraps.FirstOrDefault(x => 
                x.ServiceId == serviceId && 
                x.Active);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Bootstrap> Insert(Bootstrap bootstrap)
        {
            try
            {
                _dbContext.Bootstraps.Add(bootstrap);
                await _dbContext.SaveChangesAsync();
                return bootstrap;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
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
