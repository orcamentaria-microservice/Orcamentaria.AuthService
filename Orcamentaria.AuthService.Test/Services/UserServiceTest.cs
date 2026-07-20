using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(UserCollection))]
    public class UserServiceTest
    {
        private readonly UserFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.UserService _service;

        public UserServiceTest(UserFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.UserService>(true);
        }

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var repositoryResponse = _fixture.CreateEntity(id);

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetByIdAsync(id);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IUserRepository<User>>()
                .Verify(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()), Times.Once());
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetByIdAsync(1);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region GetByEmail

        [Fact]
        public void GetByEmail_WhenHaveData_ReturnsData()
        {
            var email = "test@test.com";
            var repositoryResponse = _fixture.CreateEntity(1);

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByEmail(email))
                .Returns(repositoryResponse);

            var response = _service.GetByEmail(email);

            response.Should().NotBeNull();
            response.Should().BeSameAs(repositoryResponse);

            _mocker.GetMock<IUserRepository<User>>()
                .Verify(r => r.GetByEmail(email), Times.Once());
        }

        [Fact]
        public void GetByEmail_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByEmail(It.IsAny<string>()))
                .Throws(new Exception());

            Action act = () => _service.GetByEmail("test@test.com");

            act.Should().Throw<UnexpectedException>();
        }

        #endregion

        #region GetAsync

        [Fact]
        public async Task GetAsync_WhenHaveData_ReturnsSuccessTrue()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (
                new List<User>() { new User { Id = 1 } },
                new ResponsePagination(1, 10, 1)
            );

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.GetAsync(gridParams);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IUserRepository<User>>()
                .Verify(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<User, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repositoryResponse = (new List<User>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task GetAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetAsync(gridParams, It.IsAny<Expression<Func<User, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region InsertAsync

        [Fact]
        public async Task InsertAsync_WhenValidBody_ReturnsSuccessTrue()
        {
            var serviceRequest = new UserInsertDTO { Password = "plainPassword" };
            var mapperInsertToEntity = new User { Password = "plainPassword" };
            var validationResult = new ValidationResult();
            var encriptedPassword = "encriptedPassword";
            var companyId = 10L;
            var repositoryResponse = new User { Id = 1, Name = "test" };
            var mapperResponseDTO = new UserResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<UserInsertDTO, User>(serviceRequest))
                .Returns(mapperInsertToEntity);

            _mocker.GetMock<IValidatorEntity<User>>()
                .Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity))
                .Returns(validationResult);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.Encript(mapperInsertToEntity.Password))
                .Returns(encriptedPassword);

            _mocker.GetMock<IUserAuthContext>()
                .Setup(c => c.UserId)
                .Returns(companyId);

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.InsertAsync(mapperInsertToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<User, UserResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.InsertAsync(serviceRequest);

            response.Success.Should().BeTrue();
            mapperInsertToEntity.Password.Should().Be(encriptedPassword);
            mapperInsertToEntity.CompanyId.Should().Be(companyId);
            _mocker.GetMock<IUserRepository<User>>().Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new UserInsertDTO();
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Name", "Error") } };

            _mocker.GetMock<IMapper>().Setup(m => m.Map<UserInsertDTO, User>(serviceRequest)).Returns(new User());
            _mocker.GetMock<IValidatorEntity<User>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<User>())).Returns(validationResult);

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task InsertAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new UserInsertDTO();
            var mapperInsertToEntity = new User();
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<UserInsertDTO, User>(serviceRequest)).Returns(mapperInsertToEntity);
            _mocker.GetMock<IValidatorEntity<User>>().Setup(v => v.ValidateBeforeInsert(mapperInsertToEntity)).Returns(validationResult);
            _mocker.GetMock<IPasswordService>().Setup(p => p.Encript(It.IsAny<string>())).Returns("encriptedPassword");
            _mocker.GetMock<IUserAuthContext>().Setup(c => c.UserId).Returns(1);
            _mocker.GetMock<IUserRepository<User>>().Setup(r => r.InsertAsync(mapperInsertToEntity)).ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.InsertAsync(serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region UpdateAsync

        [Xunit.Theory]
        [InlineData(1)]
        public async Task UpdateAsync_WhenValidParameterAndBody_ReturnsSuccessTrue(long id)
        {
            var serviceRequest = new UserUpdateDTO();
            var mapperUpdateToEntity = new User();
            var validationResult = new ValidationResult();
            var repositoryResponse = new User { Id = id };
            var mapperResponseDTO = new UserResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<UserUpdateDTO, User>(serviceRequest))
                .Returns(mapperUpdateToEntity);

            _mocker.GetMock<IValidatorEntity<User>>()
                .Setup(v => v.ValidateBeforeUpdate(It.IsAny<User>()))
                .Returns(validationResult);

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.UpdateAsync(id, mapperUpdateToEntity))
                .ReturnsAsync(repositoryResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<User, UserResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.UpdateAsync(id, serviceRequest);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IUserRepository<User>>().Verify(r => r.UpdateAsync(id, It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidBody_ThrowsValidationException()
        {
            var serviceRequest = new UserUpdateDTO();
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Name", "Error") } };

            _mocker.GetMock<IMapper>().Setup(m => m.Map<UserUpdateDTO, User>(serviceRequest)).Returns(new User());
            _mocker.GetMock<IValidatorEntity<User>>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<User>())).Returns(validationResult);

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenMapperThrowsException_ThrowsUnexpectedException()
        {
            var serviceRequest = new UserUpdateDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<UserUpdateDTO, User>(serviceRequest))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var serviceRequest = new UserUpdateDTO();
            var mapperUpdateToEntity = new User();
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<UserUpdateDTO, User>(serviceRequest)).Returns(mapperUpdateToEntity);
            _mocker.GetMock<IValidatorEntity<User>>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<User>())).Returns(validationResult);
            _mocker.GetMock<IUserRepository<User>>().Setup(r => r.UpdateAsync(1, mapperUpdateToEntity)).ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.UpdateAsync(1, serviceRequest);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region UpdatePasswordAsync

        [Fact]
        public async Task UpdatePasswordAsync_WhenUserIdDiffersFromContext_ThrowsInfoException()
        {
            var dto = new UserUpdatePasswordDTO { Password = "NewPassword123" };

            _mocker.GetMock<IUserAuthContext>()
                .Setup(c => c.UserId)
                .Returns(2);

            Func<Task> act = async () => await _service.UpdatePasswordAsync(1, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.Unauthorized);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenUserNotFound_ThrowsInfoException()
        {
            var id = 1;
            var dto = new UserUpdatePasswordDTO { Password = "NewPassword123" };

            _mocker.GetMock<IUserAuthContext>().Setup(c => c.UserId).Returns(id);
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _service.UpdatePasswordAsync(id, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenPasswordPatternInvalid_ThrowsValidationException()
        {
            var id = 1;
            var dto = new UserUpdatePasswordDTO { Password = "weak" };
            var user = _fixture.CreateEntity(id);
            var validationResult = new ValidationResult { Errors = { new ValidationFailure("Password", "Error") } };

            _mocker.GetMock<IUserAuthContext>().Setup(c => c.UserId).Returns(id);
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(dto.Password))
                .Returns(validationResult);

            Func<Task> act = async () => await _service.UpdatePasswordAsync(id, dto);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenValid_ReturnsSuccessTrue()
        {
            var id = 1;
            var dto = new UserUpdatePasswordDTO { Password = "NewPassword123" };
            var user = _fixture.CreateEntity(id);
            var validationResult = new ValidationResult();
            var encriptedPassword = "encriptedPassword";
            var repositoryResponse = _fixture.CreateEntity(id);
            var mapperResponseDTO = new UserResponseDTO();

            _mocker.GetMock<IUserAuthContext>().Setup(c => c.UserId).Returns(id);
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);
            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(dto.Password))
                .Returns(validationResult);
            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.Encript(dto.Password))
                .Returns(encriptedPassword);
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.UpdatePasswordAsync(id, encriptedPassword))
                .ReturnsAsync(repositoryResponse);
            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<User, UserResponseDTO>(repositoryResponse))
                .Returns(mapperResponseDTO);

            var response = await _service.UpdatePasswordAsync(id, dto);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IUserRepository<User>>().Verify(r => r.UpdatePasswordAsync(id, encriptedPassword), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var id = 1;
            var dto = new UserUpdatePasswordDTO { Password = "NewPassword123" };

            _mocker.GetMock<IUserAuthContext>().Setup(c => c.UserId).Returns(id);
            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.UpdatePasswordAsync(id, dto);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region AddPermissionsAsync

        [Fact]
        public async Task AddPermissionsAsync_WhenUserNotFound_ThrowsInfoException()
        {
            var userId = 1;
            var dto = new UserAddPermissionsDTO { PermissionIds = new List<long> { 1 } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _service.AddPermissionsAsync(userId, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task AddPermissionsAsync_WhenPermissionNotFound_ThrowsInfoException()
        {
            var userId = 1;
            var permissionId = 1;
            var user = _fixture.CreateEntity(userId);
            var dto = new UserAddPermissionsDTO { PermissionIds = new List<long> { permissionId } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);

            _mocker.GetMock<IPermissionService>()
                .Setup(p => p.GetByIdAsync(permissionId))
                .ReturnsAsync((Permission?)null);

            Func<Task> act = async () => await _service.AddPermissionsAsync(userId, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task AddPermissionsAsync_WhenValid_ReturnsSuccessTrue()
        {
            var userId = 1;
            var user = _fixture.CreateEntity(userId);
            var permissions = _fixture.CreatePermissions(2);
            var dto = new UserAddPermissionsDTO { PermissionIds = permissions.Select(p => p.Id) };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);

            foreach (var permission in permissions)
            {
                _mocker.GetMock<IPermissionService>()
                    .Setup(p => p.GetByIdAsync(permission.Id))
                    .ReturnsAsync(permission);
            }

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.AddPermissionsAsync(userId, It.IsAny<IEnumerable<Permission>>()))
                .ReturnsAsync(user);

            var response = await _service.AddPermissionsAsync(userId, dto);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IUserRepository<User>>().Verify(r => r.AddPermissionsAsync(userId, It.IsAny<IEnumerable<Permission>>()), Times.Once);
        }

        [Fact]
        public async Task AddPermissionsAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var userId = 1;
            var dto = new UserAddPermissionsDTO { PermissionIds = new List<long> { 1 } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.AddPermissionsAsync(userId, dto);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region RemovePermissionsAsync

        [Fact]
        public async Task RemovePermissionsAsync_WhenUserNotFound_ThrowsInfoException()
        {
            var userId = 1;
            var dto = new UserRemovePermissionsDTO { PermissionIds = new List<long> { 1 } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _service.RemovePermissionsAsync(userId, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task RemovePermissionsAsync_WhenPermissionNotFound_ThrowsInfoException()
        {
            var userId = 1;
            var permissionId = 1;
            var user = _fixture.CreateEntity(userId);
            var dto = new UserRemovePermissionsDTO { PermissionIds = new List<long> { permissionId } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);

            _mocker.GetMock<IPermissionService>()
                .Setup(p => p.GetByIdAsync(permissionId))
                .ReturnsAsync((Permission?)null);

            Func<Task> act = async () => await _service.RemovePermissionsAsync(userId, dto);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task RemovePermissionsAsync_WhenValid_ReturnsSuccessTrue()
        {
            var userId = 1;
            var user = _fixture.CreateEntity(userId);
            var permissions = _fixture.CreatePermissions(2);
            var dto = new UserRemovePermissionsDTO { PermissionIds = permissions.Select(p => p.Id) };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(user);

            foreach (var permission in permissions)
            {
                _mocker.GetMock<IPermissionService>()
                    .Setup(p => p.GetByIdAsync(permission.Id))
                    .ReturnsAsync(permission);
            }

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.RemovePermissionsAsync(userId, It.IsAny<IEnumerable<Permission>>()))
                .ReturnsAsync(user);

            var response = await _service.RemovePermissionsAsync(userId, dto);

            response.Success.Should().BeTrue();
            _mocker.GetMock<IUserRepository<User>>().Verify(r => r.RemovePermissionsAsync(userId, It.IsAny<IEnumerable<Permission>>()), Times.Once);
        }

        [Fact]
        public async Task RemovePermissionsAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var userId = 1;
            var dto = new UserRemovePermissionsDTO { PermissionIds = new List<long> { 1 } };

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.RemovePermissionsAsync(userId, dto);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}