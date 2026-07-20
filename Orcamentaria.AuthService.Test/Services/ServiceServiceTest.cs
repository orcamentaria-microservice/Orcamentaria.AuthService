using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;
using System.Linq.Expressions;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(Fixtures.ServiceCollection))]
    public class ServiceServiceTest
    {
        private readonly ServiceFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.ServiceService _service;

        public ServiceServiceTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _mocker.GetMock<IServiceProvider>().As<IKeyedServiceProvider>();
            _service = _mocker.CreateInstance<Application.Services.ServiceService>(true);
        }

        #region GetAsync

        [Fact]
        public async Task GetAsync_WhenHaveData_ReturnsSuccessTrue()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (
                new List<Service>() { _fixture.CreateEntity(1) },
                new ResponsePagination(1, 10, 1)
            );

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetAsync(gridParams);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IServiceRepository<Service>>()
                .Verify(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Service, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (new List<Service>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task GetAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region GetByCredentialsAsync

        [Fact]
        public async Task GetByCredentialsAsync_WhenHaveData_ReturnsData()
        {
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var repositoryResponse = _fixture.CreateEntity(1);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByCredentialsAsync(clientId, clientSecret))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetByCredentialsAsync(clientId, clientSecret);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Verify(r => r.GetByCredentialsAsync(clientId, clientSecret), Times.Once());
        }

        [Fact]
        public async Task GetByCredentialsAsync_WhenNotFound_ThrowsInfoException()
        {
            var clientId = "clientId";
            var clientSecret = "clientSecret";

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByCredentialsAsync(clientId, clientSecret))
                .ReturnsAsync((Service?)null);

            Func<Task> act = async () => await _service.GetByCredentialsAsync(clientId, clientSecret);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task GetByCredentialsAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetByCredentialsAsync("clientId", "clientSecret");

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var repositoryResponse = _fixture.CreateEntity(id);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetByIdAsync(id);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Verify(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Service, object>>[]>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Service, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetByIdAsync(1);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region InsertAsync

        [Fact]
        public async Task InsertAsync_WhenValidBody_ReturnsSuccessTrue()
        {
            var serviceRequest = new ServiceInsertDTO { Name = "test" };
            var mapperInsertToEntity = new Service { Name = "test" };
            var validationResult = new ValidationResult();
            var clientId = "generatedClientId";
            var clientSecret = "generatedClientSecret";
            var repositoryResponse = new Service { Id = 1, Name = "test" };
            var mapperResponseDTO = new ServiceResponseDTO();

            var clientIdTokenMock = new Mock<ITokenService<Service>>();
            var clientSecretTokenMock = new Mock<ITokenService<Service>>();

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientIdToken"))
                .Returns(clientIdTokenMock.Object);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientSecretToken"))
                .Returns(clientSecretTokenMock.Object);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<ServiceInsertDTO, Service>(serviceRequest))
                .Returns(mapperInsertToEntity);

            clientIdTokenMock
                .Setup(t => t.Generate(mapperInsertToEntity))
                .Returns(clientId);

            clientSecretTokenMock
                .Setup(t => t.Generate(mapperInsertToEntity))
                .Returns(clientSecret);

            _mocker.GetMock<IValidatorEntity<Service>>()
                .Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity))
                .Returns(validationResult);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.InsertAsync(mapperInsertToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Service, ServiceResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.InsertAsync(serviceRequest);

            response.Success.Should().BeTrue();
            mapperInsertToEntity.ClientId.Should().Be(clientId);
            mapperInsertToEntity.ClientSecret.Should().Be(clientSecret);
            _mocker.GetMock<IServiceRepository<Service>>().Verify(r => r.InsertAsync(It.IsAny<Service>()), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new ServiceInsertDTO();
            var mapperInsertToEntity = new Service();
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Name", "Error") } };

            var clientIdTokenMock = new Mock<ITokenService<Service>>();
            var clientSecretTokenMock = new Mock<ITokenService<Service>>();

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientIdToken"))
                .Returns(clientIdTokenMock.Object);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientSecretToken"))
                .Returns(clientSecretTokenMock.Object);

            _mocker.GetMock<IMapper>().Setup(m => m.Map<ServiceInsertDTO, Service>(serviceRequest)).Returns(mapperInsertToEntity);
            _mocker.GetMock<IValidatorEntity<Service>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Service>())).Returns(validationResult);

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task InsertAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new ServiceInsertDTO();
            var mapperInsertToEntity = new Service();
            var validationResult = new ValidationResult();

            var clientIdTokenMock = new Mock<ITokenService<Service>>();
            var clientSecretTokenMock = new Mock<ITokenService<Service>>();

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientIdToken"))
                .Returns(clientIdTokenMock.Object);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientSecretToken"))
                .Returns(clientSecretTokenMock.Object);

            _mocker.GetMock<IMapper>().Setup(m => m.Map<ServiceInsertDTO, Service>(serviceRequest)).Returns(mapperInsertToEntity);
            _mocker.GetMock<IValidatorEntity<Service>>().Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity)).Returns(validationResult);
            _mocker.GetMock<IServiceRepository<Service>>().Setup(r => r.InsertAsync(mapperInsertToEntity)).ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region UpdateAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task UpdateAsync_WhenValidParameterAndBody_ReturnsSuccessTrue(long id)
        {
            var serviceRequest = new ServiceUpdateDTO();
            var mapperUpdateToEntity = new Service();
            var validationResult = new ValidationResult();
            var repositoryResponse = new Service { Id = id };
            var mapperResponseDTO = new ServiceResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<ServiceUpdateDTO, Service>(serviceRequest))
                .Returns(mapperUpdateToEntity);

            _mocker.GetMock<IValidatorEntity<Service>>()
                .Setup(v => v.ValidateBeforeInsert(It.IsAny<Service>()))
                .Returns(validationResult);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.UpdateAsync(id, mapperUpdateToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Service, ServiceResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.UpdateAsync(id, serviceRequest);

            response.Success.Should().BeTrue();
            mapperUpdateToEntity.Id.Should().Be(id);
            _mocker.GetMock<IServiceRepository<Service>>().Verify(r => r.UpdateAsync(id, It.IsAny<Service>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new ServiceUpdateDTO();
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Name", "Error") } };

            _mocker.GetMock<IMapper>().Setup(m => m.Map<ServiceUpdateDTO, Service>(serviceRequest)).Returns(new Service());
            _mocker.GetMock<IValidatorEntity<Service>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Service>())).Returns(validationResult);

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenMapperThrowsException_ThrowsUnexpectedException()
        {
            var serviceRequest = new ServiceUpdateDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<ServiceUpdateDTO, Service>(serviceRequest))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new ServiceUpdateDTO();
            var mapperUpdateToEntity = new Service();
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<ServiceUpdateDTO, Service>(serviceRequest)).Returns(mapperUpdateToEntity);
            _mocker.GetMock<IValidatorEntity<Service>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Service>())).Returns(validationResult);
            _mocker.GetMock<IServiceRepository<Service>>().Setup(r => r.UpdateAsync(1, mapperUpdateToEntity)).ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region UpdateCredentialsAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task UpdateCredentialsAsync_WhenValid_ReturnsSuccessTrue(long id)
        {
            var entity = _fixture.CreateEntity(id);
            var clientId = "generatedClientId";
            var clientSecret = "generatedClientSecret";
            var mapperResponseDTO = new ServiceResponseDTO();

            var clientIdTokenMock = new Mock<ITokenService<Service>>();
            var clientSecretTokenMock = new Mock<ITokenService<Service>>();

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientIdToken"))
                .Returns(clientIdTokenMock.Object);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientSecretToken"))
                .Returns(clientSecretTokenMock.Object);

            clientIdTokenMock.Setup(t => t.Generate(entity)).Returns(clientId);
            clientSecretTokenMock.Setup(t => t.Generate(entity)).Returns(clientSecret);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.UpdateAsync(id, entity))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Service, ServiceResponseDTO>(entity))
                .Returns(mapperResponseDTO);

            var response = await _service.UpdateCredentialsAsync(id);

            response.Success.Should().BeTrue();
            entity.ClientId.Should().Be(clientId);
            entity.ClientSecret.Should().Be(clientSecret);
            _mocker.GetMock<IServiceRepository<Service>>().Verify(r => r.UpdateAsync(id, entity), Times.Once);
        }

        [Fact]
        public async Task UpdateCredentialsAsync_WhenNotFound_ThrowsInfoException()
        {
            var id = 1;

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync((Service?)null);

            Func<Task> act = async () => await _service.UpdateCredentialsAsync(id);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task UpdateCredentialsAsync_WhenSecondGetByIdReturnsNull_ThrowsInfoException()
        {
            var id = 1;
            var entity = _fixture.CreateEntity(id);
            var callCount = 0;

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? entity : null;
                });

            var clientIdTokenMock = new Mock<ITokenService<Service>>();
            var clientSecretTokenMock = new Mock<ITokenService<Service>>();

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientIdToken"))
                .Returns(clientIdTokenMock.Object);

            _mocker.GetMock<IServiceProvider>()
                .As<IKeyedServiceProvider>()
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "clientSecretToken"))
                .Returns(clientSecretTokenMock.Object);

            Func<Task> act = async () => await _service.UpdateCredentialsAsync(id);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task UpdateCredentialsAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Service, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.UpdateCredentialsAsync(1);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
