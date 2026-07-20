using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.DTOs.Permission;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(PermissionCollection))]
    public class PermissionServiceTest
    {
        private readonly PermissionFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.PermissionService _service;

        public PermissionServiceTest(PermissionFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.PermissionService>(true);
        }

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var repositoryResponse = _fixture.CreateEntity(id);

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetByIdAsync(id);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Verify(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<Permission, object>>[]>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetByIdAsync(1);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region GetAsync

        [Fact]
        public async Task GetAsync_WhenHaveData_ReturnsSuccessTrue()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (
                new List<Permission>() { _fixture.CreateEntity(1) },
                new ResponsePagination(1, 10, 1)
            );

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetAsync(gridParams);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Verify(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Permission, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (new List<Permission>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task GetAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region InsertAsync

        [Fact]
        public async Task InsertAsync_WhenValidBody_ReturnsSuccessTrue()
        {
            var serviceRequest = new PermissionInsertDTO { IncrementalPermission = "resource.action" };
            var mapperInsertToEntity = new Permission { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult();
            var repositoryResponse = _fixture.CreateEntity(1);
            var mapperResponseDTO = new PermissionResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionInsertDTO, Permission>(serviceRequest))
                .Returns(mapperInsertToEntity);

            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity))
                .Returns(validationResult);

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.InsertAsync(mapperInsertToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Permission, PermissionResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.InsertAsync(serviceRequest);

            response.Success.Should().BeTrue();
            mapperInsertToEntity.IncrementalPermission.Should().Be("RESOURCE.ACTION");
            _mocker.GetMock<IPermissionRepository<Permission>>().Verify(r => r.InsertAsync(It.IsAny<Permission>()), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new PermissionInsertDTO { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Resource", "Error") } };

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionInsertDTO, Permission>(serviceRequest))
                .Returns(new Permission { IncrementalPermission = "resource.action" });
            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeInsert(It.IsAny<Permission>()))
                .Returns(validationResult);

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task InsertAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new PermissionInsertDTO { IncrementalPermission = "resource.action" };
            var mapperInsertToEntity = new Permission { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionInsertDTO, Permission>(serviceRequest))
                .Returns(mapperInsertToEntity);
            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity))
                .Returns(validationResult);
            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.InsertAsync(mapperInsertToEntity))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region UpdateAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task UpdateAsync_WhenValidParameterAndBody_ReturnsSuccessTrue(long id)
        {
            var serviceRequest = new PermissionUpdateDTO { IncrementalPermission = "resource.action" };
            var mapperUpdateToEntity = new Permission { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult();
            var repositoryResponse = _fixture.CreateEntity(id);
            var mapperResponseDTO = new PermissionResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionUpdateDTO, Permission>(serviceRequest))
                .Returns(mapperUpdateToEntity);

            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeUpdate(It.IsAny<Permission>()))
                .Returns(validationResult);

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.UpdateAsync(id, mapperUpdateToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Permission, PermissionResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.UpdateAsync(id, serviceRequest);

            response.Success.Should().BeTrue();
            mapperUpdateToEntity.IncrementalPermission.Should().Be("RESOURCE.ACTION");
            mapperUpdateToEntity.Id.Should().Be(id);
            _mocker.GetMock<IPermissionRepository<Permission>>().Verify(r => r.UpdateAsync(id, It.IsAny<Permission>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new PermissionUpdateDTO { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Resource", "Error") } };

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionUpdateDTO, Permission>(serviceRequest))
                .Returns(new Permission { IncrementalPermission = "resource.action" });
            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeUpdate(It.IsAny<Permission>()))
                .Returns(validationResult);

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new PermissionUpdateDTO { IncrementalPermission = "resource.action" };
            var mapperUpdateToEntity = new Permission { IncrementalPermission = "resource.action" };
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<PermissionUpdateDTO, Permission>(serviceRequest))
                .Returns(mapperUpdateToEntity);
            _mocker.GetMock<IValidatorEntity<Permission>>()
                .Setup(v => v.ValidateBeforeUpdate(It.IsAny<Permission>()))
                .Returns(validationResult);
            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.UpdateAsync(1, mapperUpdateToEntity))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
