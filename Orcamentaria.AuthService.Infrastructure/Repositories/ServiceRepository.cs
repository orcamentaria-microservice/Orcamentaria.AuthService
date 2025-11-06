using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class ServiceRepository : BasicRepository<Service>, IServiceRepository
    {
        private readonly MySqlContext _dbContext;

        public ServiceRepository(
            MySqlContext dbContext, 
            IUserAuthContext userAuthContext) 
            : base(dbContext, userAuthContext)
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
    }
}
