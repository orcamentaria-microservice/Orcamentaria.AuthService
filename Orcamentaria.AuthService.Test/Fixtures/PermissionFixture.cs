using Bogus;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Test.Fixtures;

namespace Orcamentaria.AuthService.Test.Fixtures
{
    [CollectionDefinition(nameof(PermissionCollection))]
    public class PermissionCollection : ICollectionFixture<PermissionFixture> { }

    public class PermissionFixture : BaseFixture<Permission>
    {
        override
        public Permission CreateEntity(long id)
        {
            return new Permission
            {
                Id = id,
                Resource = (ResourceEnum)Faker.Random.Int(1, 5),
                Description = Faker.Lorem.Sentence(),
                Type = (PermissionTypeEnum)Faker.Random.Int(1, 3),
                IncrementalPermission = String.Empty,
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
                UpdatedAt = Faker.Date.Future(),
                UpdatedBy = 1
            };
        }
    }
}