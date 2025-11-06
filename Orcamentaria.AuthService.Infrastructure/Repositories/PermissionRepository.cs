using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.Lib.Infrastructure.Repositories;
using Orcamentaria.Lib.Domain.Contexts;

namespace Orcamentaria.AuthService.Infrastructure.Repositories
{
    public class PermissionRepository : BasicRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(
            MySqlContext dbContext, 
            IUserAuthContext userAuthContext) 
            : base(dbContext, userAuthContext)
        {
        }
    }
}
