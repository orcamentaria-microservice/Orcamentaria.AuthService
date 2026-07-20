using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Infrastructure.Contexts;

namespace Orcamentaria.AuthService.Test.Contexts
{
    public class MySqlContextTest : MySqlContext
    {
        public MySqlContextTest(DbContextOptions<DbContext> options) : base(options) { }
    }
}
