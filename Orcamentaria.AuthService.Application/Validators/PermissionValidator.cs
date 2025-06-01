using FluentValidation;
using FluentValidation.Results;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Validators
{
    public class PermissionValidator : AbstractValidator<Permission>, IValidatorEntity<Permission>
    {
        private readonly IPermissionRepository _repository;

        public PermissionValidator(
            IPermissionRepository permissionRepository)
        {
            _repository = permissionRepository;
        }

        public PermissionValidator()
        {
            RuleFor(x => x.Resource)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .Must(x => Enum.IsDefined(typeof(ResourceEnum), x)).WithMessage("O {PropertyName} é inválido.");
            RuleFor(x => x.Description)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.")
                .MaximumLength(150).WithMessage("O tamanho máximo da {PropertyName} é de {MaxLength} caracteres.");
            RuleFor(x => x.Type)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.")
                .Must(x => Enum.IsDefined(typeof(PermissionTypeEnum), x)).WithMessage("O {PropertyName} é inválido.");
        }

        public ValidationResult ValidateBeforeInsert(Permission entity)
        {
            var validator = new PermissionValidator();

            validator.RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} não deve ser informado.");

            validator.RuleFor(x => x.CreateAt)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.");

            return validator.Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(Permission entity)
        {
            var validator = new PermissionValidator();

            validator.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O {PropertyName} deve ser informado.");

            validator.RuleFor(x => x.Id)
               .Must((x, cancelation) =>
               {
                   var entity = _repository.GetById(x.Id);

                   return entity is not null;

               }).WithMessage("Id não encontrado.");

            return validator.Validate(entity);
        }
    }
}
