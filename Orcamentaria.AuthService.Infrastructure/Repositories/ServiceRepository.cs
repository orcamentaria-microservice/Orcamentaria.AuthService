using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository<Service>
    {
        private readonly MySqlContext _dbContext;

        public ServiceRepository(
            MySqlContext dbContext, 
            IUserAuthContext userAuthContext) 
            : base(dbContext, userAuthContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Service?> GetByCredentialsAsync(string clientId, string clientSecret)
        {
            try
            {
                return await _dbContext.Services.FirstOrDefaultAsync(
                    x => x.ClientId == clientId && x.ClientSecret == clientSecret);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
    }
}
