using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.AuthService.Test.Contexts;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Infrastructure.Contexts;
using Orcamentaria.Lib.Test.Repositories;

namespace Orcamentaria.AuthService.Test.Repositories
{
    [Collection(nameof(ServiceCollection))]
    public class ServiceRepositoryTest
    {
        private readonly ServiceFixture _fixture;
        private readonly MySqlContextTest _dbContext;
        private readonly DbContextOptions<DbContext> _options;
        private readonly UserAuthContext _userAuthContext;
        public ServiceRepositoryTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _userAuthContext = _fixture.CreateUserAuthContext();

            _options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

            _dbContext = new MySqlContextTest(_options);
        }

        #region GetByCredentialsAsync
        [Xunit.Theory]
        [InlineData("1234", "1234")]
        [InlineData("5678", "5678")]
        [InlineData("9012", "5678")]
        public async Task GetByCredentialsAsync_WhenClientIdAndClientSecretMismatch_ReturnsNull(string clientId, string clientSecret)
        {
            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("1234a", "1234")]
        [InlineData("5678b", "5678")]
        [InlineData("9012c", "5678")]
        public async Task GetByCredentialsAsync_WhenClientIdMismatchCaseSensitive_ReturnsNull(string clientId, string clientSecret)
        {
            var entity = _fixture.CreateEntity(1);

            entity.ClientId = clientId.ToUpper();
            entity.ClientSecret = clientSecret;

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entity);

            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("1234a", "1234a")]
        [InlineData("5678b", "5678b")]
        [InlineData("9012c", "9012c")]
        public async Task GetByCredentialsAsync_WhenClientSecretMismatchCaseSensitive_ReturnsNull(string clientId, string clientSecret)
        {
            var entity = _fixture.CreateEntity(1);

            entity.ClientId = clientId;
            entity.ClientSecret = clientSecret.ToUpper();

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entity);

            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("1234a", "1234")]
        [InlineData("5678b", "5678")]
        [InlineData("9012c", "5678")]
        public async Task GetByCredentialsAsync_WhenClientIdMismatch_ReturnsNull(string clientId, string clientSecret)
        {
            var entity = _fixture.CreateEntity(1);

            entity.ClientId = "mismatch";
            entity.ClientSecret = clientSecret;

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entity);

            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("1234a", "1234a")]
        [InlineData("5678b", "5678b")]
        [InlineData("9012c", "9012c")]
        public async Task GetByCredentialsAsync_WhenClientSecretMismatch_ReturnsNull(string clientId, string clientSecret)
        {
            var entity = _fixture.CreateEntity(1);

            entity.ClientId = clientId;
            entity.ClientSecret = "mismatch";

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entity);

            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("1234a", "1234a")]
        [InlineData("5678b", "5678b")]
        [InlineData("9012c", "9012c")]
        public async Task GetByCredentialsAsync_WhenClientIdAndClientSecretMatch_ReturnsData(string clientId, string clientSecret)
        {
            var entity = _fixture.CreateEntity(1);

            entity.ClientId = clientId;
            entity.ClientSecret = clientSecret;

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entity);

            var mockRepository = new Mock<ServiceRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.GetByCredentialsAsync(clientId, clientSecret);

            result.Should().NotBeNull();
        }
        #endregion
    }

    [Collection(nameof(ServiceCollection))]
    public class ServiceReadRepositoryTest : ReadWithoutCompanyRepositoryTests<Service, MySqlContextTest>
    {
        public ServiceReadRepositoryTest(ServiceFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(ServiceCollection))]
    public class ServiceWriteRepositoryTest : WriteWithCompanyRepositoryTests<Service, MySqlContextTest>
    {
        public ServiceWriteRepositoryTest(ServiceFixture fixture) : base(fixture) { }
    }
}
