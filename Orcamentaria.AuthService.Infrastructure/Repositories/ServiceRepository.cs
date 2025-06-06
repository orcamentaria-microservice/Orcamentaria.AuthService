using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly MySqlContext _dbContext;

        public ServiceRepository(MySqlContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Service? GetByCredentials(string clientId, string clientSecret)
        {
            try
            {
                return _dbContext.Services.FirstOrDefault(
                    x => x.ClientId == clientId && x.ClientSecret == clientSecret);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public Service? GetById(long id)
        {
            try
            {
                return _dbContext.Services.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Service> Insert(Service service)
        {
            try
            {
                _dbContext.Services.Add(service);
                await _dbContext.SaveChangesAsync();
                return service;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Service> Update(long id, Service address)
        {
            try
            {
                var entity = _dbContext.Services.First(p => p.Id == id);

                entity.Name = address.Name;
                entity.Active = address.Active;

                await _dbContext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Service> UpdateCredentials(long id, string clientId, string clientSecret)
        {
            try
            {
                var entity = _dbContext.Services.First(p => p.Id == id);

                entity.ClientId = clientId;
                entity.ClientSecret = clientSecret;

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
