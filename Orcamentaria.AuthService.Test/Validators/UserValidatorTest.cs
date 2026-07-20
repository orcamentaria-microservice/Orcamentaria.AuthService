using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Test.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Validators
{
    [Collection(nameof(UserCollection))]
    public class UserValidatorTest
    {
        private readonly UserFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly UserValidator _validator;

        public UserValidatorTest(UserFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _validator = _mocker.CreateInstance<UserValidator>(true);
        }

        private static string CreateEmailWithLength(int length)
        {
            var suffix = "@test.com";
            return new string('a', length - suffix.Length) + suffix;
        }

        #region ValidateBeforeInsert

        [Fact]
        public void ValidateBeforeInsert_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Email = CreateEmailWithLength(200);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIdIsProvided_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Email = CreateEmailWithLength(200);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id nao deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenEmailIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Email = string.Empty;

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Email e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenEmailFormatIsInvalid_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Email = "email-invalido";

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Email e invalido.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenEmailIsWithinMaxLength_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Email = "test@test.com";

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateBeforeInsert_WhenEmailExceedsMaxLength_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Email = CreateEmailWithLength(201);

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(new ValidationResult());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Email deve ter 200 caracteres.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenPasswordIsInvalid_ReturnsInvalidWithMergedErrors()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Email = "email-invalido";

            var passwordValidationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Password", "Senha invalida.")
            });

            _mocker.GetMock<IPasswordService>()
                .Setup(p => p.ValidatePattern(entity.Password))
                .Returns(passwordValidationResult);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Senha invalida.");
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id nao deve ser informado.");
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Email e invalido.");
        }

        #endregion

        #region ValidateBeforeUpdate

        [Fact]
        public void ValidateBeforeUpdate_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = "Nome valido";
            entity.CompanyId = 1;

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = "Nome valido";
            entity.CompanyId = 1;

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotFound_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = "Nome valido";
            entity.CompanyId = 1;

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync((User?)null);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Id nao encontrado.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = string.Empty;
            entity.CompanyId = 1;

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameExceedsMaxLength_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = new string('a', 101);
            entity.CompanyId = 1;

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho maximo do Name e de 100 caracteres.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenCompanyIdIsZero_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = "Nome valido";
            entity.CompanyId = 0;

            _mocker.GetMock<IUserRepository<User>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<User, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Company Id e invalido.");
        }

        #endregion
    }
}
