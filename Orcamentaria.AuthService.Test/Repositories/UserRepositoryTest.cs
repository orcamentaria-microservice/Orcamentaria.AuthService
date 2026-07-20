using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.AuthService.Test.Contexts;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Infrastructure.Contexts;
using Orcamentaria.Lib.Test.Repositories;

namespace Orcamentaria.AuthService.Test.Repositories
{
    [Collection(nameof(UserCollection))]
    public class UserRepositoryTest
    {
        private readonly UserFixture _fixture;
        private readonly MySqlContextTest _dbContext;
        private readonly DbContextOptions<DbContext> _options;
        private readonly UserAuthContext _userAuthContext;
        public UserRepositoryTest(UserFixture fixture)
        {
            _fixture = fixture;
            _userAuthContext = _fixture.CreateUserAuthContext();

            _options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

            _dbContext = new MySqlContextTest(_options);
        }

        #region UpdatePasswordAsync
        [Xunit.Theory]
        [InlineData(1, "newPassword")]
        public async Task UpdatePasswordAsync_WhenIdNotFound_ShouldThrowException(long id, string password)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.UpdatePasswordAsync(id, password);
            
            var exception = await act.Should().ThrowAsync<NotFoundException>();
            
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1, "newPassword")]
        [InlineData(2, "newPassword")]
        [InlineData(3, "newPassword")]
        public async Task UpdatePasswordAsync_WhenCompanyIdNotFound_ShouldThrowException(long id, string password)
        {
            _userAuthContext.CompanyId = 9999;

            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.UpdatePasswordAsync(id, password);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1, "newPassword")]
        [InlineData(2, "newPassword")]
        [InlineData(3, "newPassword")]
        public async Task UpdatePasswordAsync_WhenIdAndCompanyIdNotFound_ShouldThrowException(long id, string password)
        {
            _userAuthContext.CompanyId = 9999;

            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.UpdatePasswordAsync(id, password);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1, "newPassword1")]
        [InlineData(2, "newPassword2")]
        [InlineData(3, "newPassword3")]
        public async Task UpdatePasswordAsync_WhenIdAndCompanyIdFound_ShouldUpdatePassword(long id, string password)
        {
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.UpdatePasswordAsync(id, password);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Password.Should().Be(password);
        }
        #endregion

        #region AddPermissionsAsync
        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddPermissionsAsync_WhenIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddPermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddPermissionsAsync_WhenCompanyIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            _userAuthContext.CompanyId = 9999;
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddPermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddPermissionsAsync_WhenIdAndCompanyIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            _userAuthContext.CompanyId = 9999;

            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddPermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddPermissionsAsync_WhenPermissionsNotRegistered_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddPermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<DatabaseException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task AddPermissionsAsync_WhenIdAndCompanyIdFound_ShouldAddPermissions(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            await _dbContext.Permissions.AddRangeAsync(permissions);
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };
            
            var result = await mockRepository.Object.AddPermissionsAsync(id, permissions);
            
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Permissions.Should().Contain(permissions);
        }
        #endregion

        #region RemovePermissionsAsync
        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task RemovePermissionsAsync_WhenIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemovePermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task RemovePermissionsAsync_WhenCompanyIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            _userAuthContext.CompanyId = 9999;
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemovePermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task RemovePermissionsAsync_WhenIdAndCompanyIdNotFound_ShouldThrowException(long id)
        {
            var permissions = _fixture.CreatePermissions(2);
            _userAuthContext.CompanyId = 9999;

            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, 1000);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemovePermissionsAsync(id, permissions);

            var exception = await act.Should().ThrowAsync<NotFoundException>();

            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task RemovePermissionsAsync_WhenIdAndCompanyIdFound_ShouldRemovePermissions(long id)
        {
            var permissionsAdd = _fixture.CreatePermissions(5);
            var permissionsRemove = permissionsAdd.Take(2).ToList();
            await _dbContext.Permissions.AddRangeAsync(permissionsAdd);
            await _fixture.SeedInMemoryDatabaseWithIds(_dbContext, id);

            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            await mockRepository.Object.AddPermissionsAsync(id, permissionsAdd);

            var result = await mockRepository.Object.RemovePermissionsAsync(id, permissionsRemove);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Permissions.Should().NotContain(permissionsRemove);
            result.Permissions.Should().HaveCount(3);
        }
        #endregion

        #region GetByEmail
        [Xunit.Theory]
        [InlineData("test1@example.com")]
        [InlineData("test2@example.com")]
        [InlineData("test3@example.com")]
        public void GetByEmail_WhenEmailNotFound_ShouldReturnNull(string email)
        {
            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = mockRepository.Object.GetByEmail(email);

            result.Should().BeNull();
        }

        [Xunit.Theory]
        [InlineData("test1@example.com")]
        [InlineData("test2@example.com")]
        [InlineData("test3@example.com")]
        public async Task GetByEmail_WhenEmailFound_ShouldReturnUser(string email)
        {
            var entities = new List<int>(2) { 1, 2 }.Select(i =>
            {
                var user = _fixture.CreateEntity(i);
                user.Email = i == 1 ? email : user.Email;
                return user;
            });

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entities.First(), entities.Last());
            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = mockRepository.Object.GetByEmail(email);

            result.Should().NotBeNull();
            result.Email.Should().Be(email);
        }

        [Xunit.Theory]
        [InlineData("test1@example.com")]
        [InlineData("test2@example.com")]
        [InlineData("test3@example.com")]
        public async Task GetByEmail_WhenUserContainsPermissionsAndEmailFound_ShouldReturnUserWithPermissions(string email)
        {
            var userIdTest = 1;
            var permissions = _fixture.CreatePermissions(2);
            await _dbContext.Permissions.AddRangeAsync(permissions);
            var entities = new List<int>(2) { userIdTest, 2 }.Select(i =>
            {
                var user = _fixture.CreateEntity(i);
                user.Email = i == userIdTest ? email : user.Email;
                return user;
            });

            await _fixture.SeedInMemoryDatabaseWithEntities(_dbContext, entities.First(), entities.Last());
            
            var mockRepository = new Mock<UserRepository>(_dbContext, _userAuthContext) { CallBase = true };

            await mockRepository.Object.AddPermissionsAsync(userIdTest, permissions);

            var result = mockRepository.Object.GetByEmail(email);

            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Permissions.Should().NotBeNull();
            result.Permissions.Should().HaveCount(2);
            result.Permissions.Should().Contain(permissions);
        }
        #endregion
    }

    [Collection(nameof(UserCollection))]
    public class UserReadRepositoryTest : ReadWithCompanyRepositoryTests<User, MySqlContextTest>
    {
        public UserReadRepositoryTest(UserFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(UserCollection))]
    public class UserWriteRepositoryTest : WriteWithCompanyRepositoryTests<User, MySqlContextTest>
    {
        public UserWriteRepositoryTest(UserFixture fixture) : base(fixture) { }
    }
}
