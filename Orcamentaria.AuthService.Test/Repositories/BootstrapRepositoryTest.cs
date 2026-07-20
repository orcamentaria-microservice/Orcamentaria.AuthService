using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.AuthService.Test.Contexts;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Infrastructure.Contexts;
using Orcamentaria.Lib.Test.Repositories;

namespace Orcamentaria.AuthService.Test.Repositories
{
    [Collection(nameof(BootstrapCollection))]
    public class BootstrapRepositoryTest
    {
        private readonly BootstrapFixture _fixture;
        private readonly MySqlContextTest _dbContext;
        private readonly DbContextOptions<DbContext> _options;
        private readonly UserAuthContext _userAuthContext;
        public BootstrapRepositoryTest(BootstrapFixture fixture)
        {
            _fixture = fixture;
            _userAuthContext = _fixture.CreateUserAuthContext();

            _options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

            _dbContext = new MySqlContextTest(_options);
        }

        #region UpdateHash
        [Xunit.Theory]
        [InlineData(1, "newHash")]
        public async Task UpdateHash_WhenIdNotFound_ShouldThrowException(long id, string hash)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<BootstrapRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.UpdateHash(id, hash);

            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1, "newHash")]
        public async Task UpdateHash_WhenHashIsValid_ReturnsEntity(long id, string hash)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<BootstrapRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.UpdateHash(id, hash);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }
        #endregion

        #region Inactive
        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Inactive_WhenIdNotFound_ShouldThrowException(long id)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<BootstrapRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.Inactive(id);

            var exception = await act.Should().ThrowAsync<NotFoundException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Inactive_WhenIdFound_ReturnsEntity(long id)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<BootstrapRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.Inactive(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Active.Should().BeFalse();
            result.RevokedAt.Should().NotBeNull();
        }
        #endregion
    }

    [Collection(nameof(BootstrapCollection))]
    public class BootstrapReadRepositoryTest : ReadWithoutCompanyRepositoryTests<Bootstrap, MySqlContextTest>
    {
        public BootstrapReadRepositoryTest(BootstrapFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(BootstrapCollection))]
    public class BootstrapWriteRepositoryTest : WriteWithCompanyRepositoryTests<Bootstrap, MySqlContextTest>
    {
        public BootstrapWriteRepositoryTest(BootstrapFixture fixture) : base(fixture) { }
    }
}
