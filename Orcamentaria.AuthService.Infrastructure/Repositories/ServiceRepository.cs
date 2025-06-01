using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly MySqlContext _dbContext;

        public ServiceRepository(MySqlContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Service GetByCredentials(string clientId, string clientSecret)
            => _dbContext.Services.Where(x => x.ClientId == clientId && x.ClientSecret == clientSecret)
            .FirstOrDefault();

        public Service GetById(long id)
            => _dbContext.Services.Where(x => x.Id == id)
            .FirstOrDefault();

        public async Task<Service> Insert(Service service)
        {
            _dbContext.Services.Add(service);
            await _dbContext.SaveChangesAsync();
            return service;
        }

        public async Task<Service> Update(long id, Service address)
        {
            var entity = _dbContext.Services.FirstOrDefault(p => p.Id == id);

            if (entity is not null)
            {
                entity.Name = address.Name;
                entity.Active = address.Active;

                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<Service> UpdateCredentials(long id, string clientId, string clientSecret)
        {
            var entity = _dbContext.Services.FirstOrDefault(p => p.Id == id);

            if (entity is not null)
            {
                entity.ClientId = clientId;
                entity.ClientSecret = clientSecret;

                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }
    }
}
