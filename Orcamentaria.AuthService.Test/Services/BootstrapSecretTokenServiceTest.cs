using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(BootstrapCollection))]
    public class BootstrapSecretTokenServiceTest
    {
        private readonly BootstrapFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly BootstrapSecretTokenService _service;

        public BootstrapSecretTokenServiceTest(BootstrapFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<BootstrapSecretTokenService>(true);
        }

        #region Generate

        [Fact]
        public void Generate_WhenCalled_ReturnsTokenWithHash()
        {
            var bootstrap = _fixture.CreateEntity(1);

            var response = _service.Generate(bootstrap);

            response.Should().NotBeNullOrEmpty();
            response.Should().Contain(".");
            response.Should().Contain(" || ");

            var separatorParts = response.Split(" || ");
            var token = separatorParts[0];
            var hash = separatorParts[1];

            token.Split('.')[0].Should().Be(bootstrap.Id.ToString());
            hash.Length.Should().Be(64);
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_WhenTokenIsValid_ReturnsId()
        {
            var bootstrap = _fixture.CreateEntity(1);
            var generated = _service.Generate(bootstrap);
            var separatorParts = generated.Split(" || ");
            var token = separatorParts[0];
            var hash = separatorParts[1];

            var repositoryResponse = _fixture.CreateEntity(bootstrap.Id);
            repositoryResponse.Hash = hash;
            repositoryResponse.Active = true;
            repositoryResponse.ExpiresAt = DateTime.UtcNow.AddHours(1);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(bootstrap.Id, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync(repositoryResponse);

            var response = await _service.ValidateAsync(token);

            response.Should().Be(bootstrap.Id);
        }

        [Fact]
        public async Task ValidateAsync_WhenTokenHasNoTwoParts_ThrowsInfoException()
        {
            Func<Task> act = async () => await _service.ValidateAsync("invalidtoken");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenBootstrapNotFound_ThrowsInfoException()
        {
            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync((Bootstrap?)null);

            Func<Task> act = async () => await _service.ValidateAsync("1.c2VjcmV0");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenBootstrapInactive_ThrowsInfoException()
        {
            var bootstrap = _fixture.CreateEntity(1);
            bootstrap.Active = false;
            bootstrap.ExpiresAt = DateTime.UtcNow.AddHours(1);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync(bootstrap);

            Func<Task> act = async () => await _service.ValidateAsync("1.c2VjcmV0");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenBootstrapExpired_ThrowsInfoException()
        {
            var bootstrap = _fixture.CreateEntity(1);
            bootstrap.Active = true;
            bootstrap.ExpiresAt = DateTime.UtcNow.AddHours(-1);

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync(bootstrap);

            Func<Task> act = async () => await _service.ValidateAsync("1.c2VjcmV0");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenHashDoesNotMatch_ThrowsInfoException()
        {
            var bootstrap = _fixture.CreateEntity(1);
            bootstrap.Active = true;
            bootstrap.ExpiresAt = DateTime.UtcNow.AddHours(1);
            bootstrap.Hash = "different-hash";

            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ReturnsAsync(bootstrap);

            Func<Task> act = async () => await _service.ValidateAsync("1.c2VjcmV0");

            var exception = await act.Should().ThrowAsync<InfoException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);
        }

        [Fact]
        public async Task ValidateAsync_WhenExceptionOccurs_ThrowsUnexpectedException()
        {
            _mocker.GetMock<IBootstrapRepository<Bootstrap>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Bootstrap, object>>[]>()))
                .ThrowsAsync(new Exception());

            Func<Task> act = async () => await _service.ValidateAsync("1.c2VjcmV0");

            await act.Should().ThrowAsync<UnexpectedException>();
        }

        #endregion
    }
}
