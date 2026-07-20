using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Enums;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Validators
{
    [Collection(nameof(PermissionCollection))]
    public class PermissionValidatorTest
    {
        private readonly PermissionFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly PermissionValidator _validator;

        public PermissionValidatorTest(PermissionFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _validator = _mocker.CreateInstance<PermissionValidator>(true);
        }

        #region ValidateBeforeInsert

        [Fact]
        public void ValidateBeforeInsert_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;
            entity.Description = "Permissao master";
            entity.IncrementalPermission = string.Empty;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIdIsProvided_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Resource = ResourceEnum.MASTER;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id nao deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenDescriptionIsNull_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;
            entity.Description = null!;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Description e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenDescriptionExceedsMaxLength_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;
            entity.Description = new string('a', 151);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho maximo da Description e de 150 caracteres.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenResourceIsInvalid_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = (ResourceEnum)999;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Resource e invalido.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenResourceNotMasterAndTypeIsUnset_ReturnsInvalidWithTypeObrigatorioMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.COMPANY;
            entity.Type = 0;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Type e obrigatorio.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenResourceNotMasterAndTypeIsInvalidEnumValue_ReturnsInvalidWithTypeInvalidoMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.COMPANY;
            entity.Type = (PermissionTypeEnum)999;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Type e invalido.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenResourceNotMasterAndTypeIsValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.COMPANY;
            entity.Type = PermissionTypeEnum.READ;
            entity.Description = "Permissao de leitura";
            entity.IncrementalPermission = string.Empty;

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIncrementalPermissionExceedsMaxLength_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;
            entity.IncrementalPermission = new string('a', 51);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho maximo da Incremental Permission e de 50 caracteres.");
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIncrementalPermissionContainsSpaces_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;
            entity.IncrementalPermission = "PERMISSAO GERAL";

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Incremental Permission nao pode conter espacos, ex: PERMISSAO GERAL.");
        }

        #endregion

        #region ValidateBeforeUpdate

        [Fact]
        public void ValidateBeforeUpdate_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Resource = ResourceEnum.MASTER;
            entity.Description = "Permissao master";
            entity.IncrementalPermission = string.Empty;

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ReturnsAsync(entity);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdIsEmpty_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Resource = ResourceEnum.MASTER;

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id deve ser informado.");
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotFound_ReturnsInvalid()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Resource = ResourceEnum.MASTER;

            _mocker.GetMock<IPermissionRepository<Permission>>()
                .Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<Expression<Func<Permission, object>>[]>()))
                .ReturnsAsync((Permission?)null);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Id nao encontrado.");
        }

        #endregion
    }
}
