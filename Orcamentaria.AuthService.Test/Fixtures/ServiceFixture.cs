using Bogus;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Test.Fixtures;

namespace Orcamentaria.AuthService.Test.Fixtures
{
    [CollectionDefinition(nameof(ServiceCollection))]
    public class ServiceCollection : ICollectionFixture<ServiceFixture> { }

    public class ServiceFixture : BaseFixture<Service>
    {
        override
        public Service CreateEntity(long id)
        {
            return new Service
            {
                Id = id,
                Name = new Faker().Company.CompanyName(),
                ClientId = new Faker().Random.AlphaNumeric(10),
                ClientSecret = new Faker().Random.AlphaNumeric(20),
                Active = true,
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
                UpdatedAt = Faker.Date.Future(),
                UpdatedBy = 1
            };
        }
    }
}
