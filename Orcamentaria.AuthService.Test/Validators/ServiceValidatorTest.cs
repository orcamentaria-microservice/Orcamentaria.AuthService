using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Test.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Validators
{
    [Collection(nameof(ServiceCollection))]
    public class ServiceValidatorTest
    {
        private readonly ServiceFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly ServiceValidator _validator;

        public ServiceValidatorTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _validator = _mocker.CreateInstance<ServiceValidator>(true);
        }

        #region ValidateBeforeInsert

        [Fact]
        public void ValidateBeforeInsert_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIdIsProvided_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id nao deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenClientIdIsNull_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.ClientId = null!;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Client Id e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenClientSecretIsNull_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.ClientSecret = null!;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Client Secret e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = string.Empty;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameExceedsMaxLength_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = new string('a', 101);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho maximo do Name e de 100 caracteres.");
        }

        #endregion

        #region ValidateBeforeUpdate

        [Fact]
        public void ValidateBeforeUpdate_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotFound_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync((Service?)null);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Id nao encontrado.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = string.Empty;

            _mocker.GetMock<IServiceRepository<Service>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<Service, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name e obrigatorio.");
        }

        #endregion
    }
}
