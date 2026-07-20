using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Test.Contexts;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Test.Repositories;

namespace Orcamentaria.AuthService.Test.Repositories
{
    [Collection(nameof(PermissionCollection))]
    public class PermissionRepositoryTest
    {
    }

    [Collection(nameof(PermissionCollection))]
    public class PermissionReadRepositoryTest : ReadWithoutCompanyRepositoryTests<Permission, MySqlContextTest>
    {
        public PermissionReadRepositoryTest(PermissionFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(PermissionCollection))]
    public class PermissionWriteRepositoryTest : WriteWithCompanyRepositoryTests<Permission, MySqlContextTest>
    {
        public PermissionWriteRepositoryTest(PermissionFixture fixture) : base(fixture) { }
    }
}
