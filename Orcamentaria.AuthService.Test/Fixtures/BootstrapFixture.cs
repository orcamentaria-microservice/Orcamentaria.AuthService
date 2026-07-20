using Bogus;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Test.Fixtures;

namespace Orcamentaria.AuthService.Test.Fixtures
{
    [CollectionDefinition(nameof(BootstrapCollection))]
    public class BootstrapCollection : ICollectionFixture<BootstrapFixture> { }

    public class BootstrapFixture : BaseFixture<Bootstrap>
    {
        override
        public Bootstrap CreateEntity(long id)
        {
            var serviceId = new Faker().Random.Long(1, 100);

            return new Bootstrap
            {
                Id = id,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RevokedAt = null,
                Active = true,
                Hash = new Faker().Random.Hash(),
                ServiceId = serviceId,
                Service = new Service
                {
                    Id = serviceId,
                    Name = new Faker().Company.CompanyName(),
                    ClientId = new Faker().Random.AlphaNumeric(10),
                    ClientSecret = new Faker().Random.AlphaNumeric(20),
                    Active = true,
                    CreatedAt = Faker.Date.Past(),
                    CreatedBy = 1,
                    UpdatedAt = Faker.Date.Future(),
                    UpdatedBy = 1
                },
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
            };
        }
    }
}
