using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(BootstrapCollection))]
    public class BootstrapServiceTest
    {
        private readonly BootstrapFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Mock<IKeyedServiceProvider> _keyedProviderMock;
        private readonly Application.Services.BootstrapService _service;

        public BootstrapServiceTest(BootstrapFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _keyedProviderMock = _mocker.GetMock<IServiceProvider>().As<IKeyedServiceProvider>();
            _service = _mocker.CreateInstance<Application.Services.BootstrapService>(true);
        }

        #region GenerateBootstrapSecretAsync

        [Fact]
        public async Task GenerateBootstrapSecretAsync_WhenNoActiveBootstrapExists_ReturnsSuccessTrue()
        {
            var serviceId = 1;
            var service = new Service { Id = serviceId, Name = "Service Test" };
            var tokenServiceMock = new Mock<ITokenService<Bootstrap>>();
            var insertedBootstrap = new Bootstrap
            {
                Id = 10,
                ServiceId = serviceId,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(6),
                Hash = "HashPlaceholder"
            };

            _keyedProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(tokenServiceMock.Object);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByIdAsync(serviceId))
                .ReturnsAsync(service);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync((new List<Bootstrap?>(), new ResponsePagination(1, 10, 0)));

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.InsertAsync(It.IsAny<Bootstrap>()))
                .ReturnsAsync(insertedBootstrap);

            tokenServiceMock.Setup(t => t.Generate(insertedBootstrap)).Returns("secretPart || hashPart");

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.UpdateHash(insertedBootstrap.Id, "hashPart"))
                .ReturnsAsync(insertedBootstrap);

            var response = await _service.GenerateBootstrapSecretAsync(serviceId);

            response.Success.Should().BeTrue();
            response.Data.BootstrapId.Should().Be(insertedBootstrap.Id);
            response.Data.ServiceId.Should().Be(serviceId);
            response.Data.ServiceName.Should().Be(service.Name);
            response.Data.BootstrapSecret.Should().Be("secretPart");
            response.Data.ExpiresAt.Should().Be(insertedBootstrap.ExpiresAt);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.Inactive(It.IsAny<long>()), Times.Never);
            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.InsertAsync(It.IsAny<Bootstrap>()), Times.Once);
            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.UpdateHash(insertedBootstrap.Id, "hashPart"), Times.Once);
        }

        [Fact]
        public async Task GenerateBootstrapSecretAsync_WhenActiveBootstrapExists_InactivatesPreviousAndReturnsSuccessTrue()
        {
            var serviceId = 1;
            var service = new Service { Id = serviceId, Name = "Service Test" };
            var tokenServiceMock = new Mock<ITokenService<Bootstrap>>();
            var previousBootstrap = _fixture.CreateEntity(5);
            var insertedBootstrap = new Bootstrap
            {
                Id = 10,
                ServiceId = serviceId,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(6),
                Hash = "HashPlaceholder"
            };

            _keyedProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(tokenServiceMock.Object);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByIdAsync(serviceId))
                .ReturnsAsync(service);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync((new List<Bootstrap?> { previousBootstrap }, new ResponsePagination(1, 10, 1)));

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.Inactive(previousBootstrap.Id))
                .ReturnsAsync(previousBootstrap);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.InsertAsync(It.IsAny<Bootstrap>()))
                .ReturnsAsync(insertedBootstrap);

            tokenServiceMock.Setup(t => t.Generate(insertedBootstrap)).Returns("secretPart || hashPart");

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.UpdateHash(insertedBootstrap.Id, "hashPart"))
                .ReturnsAsync(insertedBootstrap);

            var response = await _service.GenerateBootstrapSecretAsync(serviceId);

            response.Success.Should().BeTrue();

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.Inactive(previousBootstrap.Id), Times.Once);
        }

        [Fact]
        public async Task GenerateBootstrapSecretAsync_WhenServiceNotFound_ThrowsInfoException()
        {
            var serviceId = 1;
            var tokenServiceMock = new Mock<ITokenService<Bootstrap>>();

            _keyedProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(tokenServiceMock.Object);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByIdAsync(serviceId))
                .ReturnsAsync((Service?)null);

            Func<Task> act = async () => await _service.GenerateBootstrapSecretAsync(serviceId);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task GenerateBootstrapSecretAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceId = 1;

            _keyedProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.GenerateBootstrapSecretAsync(serviceId);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var repositoryResponse = _fixture.CreateEntity(id);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetByIdAsync(id);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Bootstrap, object>>[]>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetByIdAsync(1);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region RevokeBootstrapSecretAsync

        [Fact]
        public async Task RevokeBootstrapSecretAsync_WhenActiveBootstrapExists_ReturnsSuccessTrue()
        {
            var serviceId = 1;
            var bootstrap = _fixture.CreateEntity(5);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync((new List<Bootstrap?> { bootstrap }, new ResponsePagination(1, 10, 1)));

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.Inactive(bootstrap.Id))
                .ReturnsAsync(bootstrap);

            var response = await _service.RevokeBootstrapSecretAsync(serviceId);

            response.Success.Should().BeTrue();
            response.Data.Should().Be(serviceId);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Verify(r => r.Inactive(bootstrap.Id), Times.Once);
        }

        [Fact]
        public async Task RevokeBootstrapSecretAsync_WhenNoActiveBootstrapExists_ThrowsInfoException()
        {
            var serviceId = 1;

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync((new List<Bootstrap?>(), new ResponsePagination(1, 10, 0)));

            Func<Task> act = async () => await _service.RevokeBootstrapSecretAsync(serviceId);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task RevokeBootstrapSecretAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceId = 1;

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.RevokeBootstrapSecretAsync(serviceId);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
