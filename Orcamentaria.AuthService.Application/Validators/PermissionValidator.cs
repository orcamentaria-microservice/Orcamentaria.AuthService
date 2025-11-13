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

        public ValidationResult ValidateBeforeInsert(Permission entity)
        {
            CommonValidation(entity);

            RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} nao deve ser informado.");

            return Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(Permission entity)
        {
            CommonValidation(entity);

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O {PropertyName} deve ser informado.");

            RuleFor(x => x.Id)
               .Must((x, cancelation) =>
               {
                   var entity = _repository.GetByIdAsync(x.Id).GetAwaiter().GetResult();

                   return entity is not null;

               }).WithMessage("Id nao encontrado.");

            return Validate(entity);
        }

        public void CommonValidation(Permission entity)
        {
            RuleFor(x => x.Resource)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.")
                .Must(x => Enum.IsDefined(typeof(ResourceEnum), x)).WithMessage("O {PropertyName} e invalido.");
            RuleFor(x => x.Description)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.")
                .MaximumLength(150).WithMessage("O tamanho maximo da {PropertyName} e de {MaxLength} caracteres.");
            RuleFor(x => x)
                .Must(x =>
                {
                    if (x.Resource == ResourceEnum.MASTER)
                        return true;

                    return x.Type == 0;
                })
                .WithMessage("O Type e obrigatorio.")
                .Must(x =>
                {
                    if (x.Resource == ResourceEnum.MASTER)
                        return true;

                    return Enum.IsDefined(typeof(PermissionTypeEnum), x.Type);
                })
                .WithMessage("O Type e invalido.");
            RuleFor(x => x.IncrementalPermission)
                .MaximumLength(50).WithMessage("O tamanho maximo da {PropertyName} e de {MaxLength} caracteres.")
                .Must(x => !x.Contains(" ")).WithMessage("O {PropertyName} nao pode conter espacos, ex: PERMISSAO GERAL.");
        }
    }
}
