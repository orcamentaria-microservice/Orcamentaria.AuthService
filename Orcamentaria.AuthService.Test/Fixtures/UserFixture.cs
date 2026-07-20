using Bogus;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Test.Fixtures;

namespace Orcamentaria.AuthService.Test.Fixtures
{
    [CollectionDefinition(nameof(UserCollection))]
    public class UserCollection : ICollectionFixture<UserFixture> { }

    public class UserFixture : BaseFixture<User>
    {
        override
        public User CreateEntity(long id)
        {
            return new User
            {
                Id = id,
                Name = Faker.Name.FirstName(),
                Email = Faker.Internet.Email(),
                Password = Faker.Random.AlphaNumeric(10),
                Active = true,
                CompanyId = 1,
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
                UpdatedAt = Faker.Date.Future(),
                UpdatedBy = 1
            };
        }

        public IEnumerable<Permission> CreatePermissions(int count)
        {
            var permissions = new List<Permission>();
            for (int i = 0; i < count; i++)
            {
                permissions.Add(new Permission
                {
                    Id = i + 1,
                    Description = $"Permission{i + 1}",
                    Resource = (ResourceEnum)Faker.Random.Int(1, 5),
                    Type = (PermissionTypeEnum)Faker.Random.Int(1, 3),
                    CreatedAt = Faker.Date.Past(),
                    CreatedBy = 1,
                    UpdatedAt = Faker.Date.Future(),
                    UpdatedBy = 1
                });
            }
            return permissions;
        }
    }
}
