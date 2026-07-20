using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(UserCollection))]
    public class AuthenticationServiceTest
    {
        private readonly UserFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Mock<IKeyedServiceProvider> _keyedServiceProviderMock;
        private readonly Application.Services.AuthenticationService _service;

        public AuthenticationServiceTest(UserFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _keyedServiceProviderMock = _mocker.GetMock<IServiceProvider>().As<IKeyedServiceProvider>();
            _service = _mocker.CreateInstance<Application.Services.AuthenticationService>(true);
        }

        #region AuthenticateServiceAsync

        [Fact]
        public async Task AuthenticateServiceAsync_WhenValidCredentials_ReturnsSuccessTrue()
        {
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var service = new Service { Id = 1, Name = "Service1" };
            var token = "generatedToken";

            var tokenServiceMock = new Mock<ITokenService<Service>>();
            tokenServiceMock.Setup(t => t.Generate(service)).Returns(token);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "serviceToken"))
                .Returns(tokenServiceMock.Object);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByCredentialsAsync(clientId, clientSecret))
                .ReturnsAsync(service);

            var response = await _service.AuthenticateServiceAsync(clientId, clientSecret);

            response.Success.Should().BeTrue();
            response.Data!.ServiceId.Should().Be(service.Id);
            response.Data.Name.Should().Be(service.Name);
            response.Data.Token.Should().Be(token);
        }

        [Fact]
        public async Task AuthenticateServiceAsync_WhenServiceNotFound_ThrowsInfoException()
        {
            var clientId = "clientId";
            var clientSecret = "clientSecret";

            var tokenServiceMock = new Mock<ITokenService<Service>>();

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "serviceToken"))
                .Returns(tokenServiceMock.Object);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByCredentialsAsync(clientId, clientSecret))
                .ReturnsAsync((Service?)null);

            Func<Task> act = async () => await _service.AuthenticateServiceAsync(clientId, clientSecret);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task AuthenticateServiceAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var clientId = "clientId";
            var clientSecret = "clientSecret";

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByCredentialsAsync(clientId, clientSecret))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.AuthenticateServiceAsync(clientId, clientSecret);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region AuthenticateUser

        [Fact]
        public void AuthenticateUser_WhenValidCredentials_ReturnsSuccessTrue()
        {
            var email = "test@test.com";
            var password = "plainPassword";
            var user = _fixture.CreateEntity(1);
            var token = "generatedToken";
            var refreshToken = "generatedRefreshToken";

            var userTokenServiceMock = new Mock<ITokenService<User>>();
            userTokenServiceMock.Setup(t => t.Generate(user)).Returns(token);

            var userRefreshTokenServiceMock = new Mock<ITokenService<User>>();
            userRefreshTokenServiceMock.Setup(t => t.Generate(user)).Returns(refreshToken);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(userTokenServiceMock.Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Returns(userRefreshTokenServiceMock.Object);

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByEmail(email))
                .Returns(user);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.PasswordIsValid(password, user.Password))
                .Returns(true);

            var response = _service.AuthenticateUser(email, password);

            response.Success.Should().BeTrue();
            response.Data!.UserId.Should().Be(user.Id);
            response.Data.CompanyId.Should().Be(user.CompanyId);
            response.Data.Name.Should().Be(user.Name);
            response.Data.Token.Should().Be(token);
            response.Data.RefreshToken.Should().Be(refreshToken);
        }

        [Fact]
        public void AuthenticateUser_WhenUserNotFound_ThrowsInfoException()
        {
            var email = "test@test.com";
            var password = "plainPassword";

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByEmail(email))
                .Returns((User?)null);

            Action act = () => _service.AuthenticateUser(email, password);

            var exception = act.Should().Throw<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public void AuthenticateUser_WhenPasswordInvalid_ThrowsInfoException()
        {
            var email = "test@test.com";
            var password = "wrongPassword";
            var user = _fixture.CreateEntity(1);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByEmail(email))
                .Returns(user);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.PasswordIsValid(password, user.Password))
                .Returns(false);

            Action act = () => _service.AuthenticateUser(email, password);

            var exception = act.Should().Throw<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public void AuthenticateUser_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var email = "test@test.com";
            var password = "plainPassword";

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByEmail(email))
                .Throws(new Exception());

            Action act = () => _service.AuthenticateUser(email, password);

            act.Should().Throw<UnexpectedException>();
        }

        #endregion

        #region RefreshTokenUserAsync

        [Fact]
        public async Task RefreshTokenUserAsync_WhenValidRefreshToken_ReturnsSuccessTrue()
        {
            var refreshToken = "refreshToken";
            var userId = 1L;
            var user = _fixture.CreateEntity(userId);
            var token = "generatedToken";
            var newRefreshToken = "newGeneratedRefreshToken";

            var userTokenServiceMock = new Mock<ITokenService<User>>();
            userTokenServiceMock.Setup(t => t.Generate(user)).Returns(token);

            var userRefreshTokenServiceMock = new Mock<ITokenService<User>>();
            userRefreshTokenServiceMock.Setup(t => t.ValidateAsync(refreshToken)).ReturnsAsync(userId);
            userRefreshTokenServiceMock.Setup(t => t.Generate(user)).Returns(newRefreshToken);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(userTokenServiceMock.Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Returns(userRefreshTokenServiceMock.Object);

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var response = await _service.RefreshTokenUserAsync(refreshToken);

            response.Success.Should().BeTrue();
            response.Data!.UserId.Should().Be(user.Id);
            response.Data.CompanyId.Should().Be(user.CompanyId);
            response.Data.Name.Should().Be(user.Name);
            response.Data.Token.Should().Be(token);
            response.Data.RefreshToken.Should().Be(newRefreshToken);
        }

        [Fact]
        public async Task RefreshTokenUserAsync_WhenUserNotFound_ThrowsInfoException()
        {
            var refreshToken = "refreshToken";
            var userId = 1L;

            var userRefreshTokenServiceMock = new Mock<ITokenService<User>>();
            userRefreshTokenServiceMock.Setup(t => t.ValidateAsync(refreshToken)).ReturnsAsync(userId);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Returns(userRefreshTokenServiceMock.Object);

            _mocker.GetMock<IUserService>()
                .Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _service.RefreshTokenUserAsync(refreshToken);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task RefreshTokenUserAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var refreshToken = "refreshToken";

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userToken"))
                .Returns(new Mock<ITokenService<User>>().Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<User>), "userRefreshToken"))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.RefreshTokenUserAsync(refreshToken);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion

        #region AuthenticateWithBootstrapSecretAsync

        [Fact]
        public async Task AuthenticateWithBootstrapSecretAsync_WhenValid_ReturnsSuccessTrue()
        {
            var bootstrapSecret = "bootstrapSecret";
            var bootstrapId = 1L;
            var bootstrap = new Bootstrap { Id = bootstrapId, ServiceId = 2 };
            var service = new Service { Id = bootstrap.ServiceId, Name = "Service1" };
            var token = "generatedToken";

            var bootstrapSecretTokenServiceMock = new Mock<ITokenService<Bootstrap>>();
            bootstrapSecretTokenServiceMock.Setup(t => t.ValidateAsync(bootstrapSecret)).ReturnsAsync(bootstrapId);

            var bootstrapTokenServiceMock = new Mock<ITokenService<Service>>();
            bootstrapTokenServiceMock
                .Setup(t => t.Generate(It.Is<Service>(s => s.Id == service.Id && s.Name == service.Name)))
                .Returns(token);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(bootstrapSecretTokenServiceMock.Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "bootstrapToken"))
                .Returns(bootstrapTokenServiceMock.Object);

            _mocker.GetMock<IBootstrapService>()
                .Setup(s => s.GetByIdAsync(bootstrapId))
                .ReturnsAsync(bootstrap);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByIdAsync(bootstrap.ServiceId))
                .ReturnsAsync(service);

            var response = await _service.AuthenticateWithBootstrapSecretAsync(bootstrapSecret);

            response.Success.Should().BeTrue();
            response.Data!.ServiceId.Should().Be(service.Id);
            response.Data.Name.Should().Be(service.Name);
            response.Data.Token.Should().Be(token);
        }

        [Fact]
        public async Task AuthenticateWithBootstrapSecretAsync_WhenBootstrapNotFound_ThrowsInfoException()
        {
            var bootstrapSecret = "bootstrapSecret";
            var bootstrapId = 1L;

            var bootstrapSecretTokenServiceMock = new Mock<ITokenService<Bootstrap>>();
            bootstrapSecretTokenServiceMock.Setup(t => t.ValidateAsync(bootstrapSecret)).ReturnsAsync(bootstrapId);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(bootstrapSecretTokenServiceMock.Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "bootstrapToken"))
                .Returns(new Mock<ITokenService<Service>>().Object);

            _mocker.GetMock<IBootstrapService>()
                .Setup(s => s.GetByIdAsync(bootstrapId))
                .ReturnsAsync((Bootstrap?)null);

            Func<Task> act = async () => await _service.AuthenticateWithBootstrapSecretAsync(bootstrapSecret);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task AuthenticateWithBootstrapSecretAsync_WhenServiceNotFound_ThrowsInfoException()
        {
            var bootstrapSecret = "bootstrapSecret";
            var bootstrapId = 1L;
            var bootstrap = new Bootstrap { Id = bootstrapId, ServiceId = 2 };

            var bootstrapSecretTokenServiceMock = new Mock<ITokenService<Bootstrap>>();
            bootstrapSecretTokenServiceMock.Setup(t => t.ValidateAsync(bootstrapSecret)).ReturnsAsync(bootstrapId);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Returns(bootstrapSecretTokenServiceMock.Object);

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Service>), "bootstrapToken"))
                .Returns(new Mock<ITokenService<Service>>().Object);

            _mocker.GetMock<IBootstrapService>()
                .Setup(s => s.GetByIdAsync(bootstrapId))
                .ReturnsAsync(bootstrap);

            _mocker.GetMock<IServiceService>()
                .Setup(s => s.GetByIdAsync(bootstrap.ServiceId))
                .ReturnsAsync((Service?)null);

            Func<Task> act = async () => await _service.AuthenticateWithBootstrapSecretAsync(bootstrapSecret);

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);
        }

        [Fact]
        public async Task AuthenticateWithBootstrapSecretAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            var bootstrapSecret = "bootstrapSecret";

            _keyedServiceProviderMock
                .Setup(p => p.GetRequiredKeyedService(typeof(ITokenService<Bootstrap>), "bootstrapSecretToken"))
                .Throws(new Exception());

            Func<Task> act = async () => await _service.AuthenticateWithBootstrapSecretAsync(bootstrapSecret);

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
