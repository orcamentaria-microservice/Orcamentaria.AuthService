using FluentValidation;
using FluentValidation.Results;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Validators
{
    public class UserValidator : AbstractValidator<User>, IValidatorEntity<User>
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordService _passwordService;

        public UserValidator(
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _repository = userRepository;
            _passwordService = passwordService;
        }

        public ValidationResult ValidateBeforeInsert(User entity)
        {
            RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} nao deve ser informado.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("O {PropertyName} e invalido.")
                .NotEmpty().WithMessage("O {PropertyName} e obrigatorio.")
                .Length(200).WithMessage("O {PropertyName} deve ter {MaxLength} caracteres.");

            var resultValidationPassword = _passwordService.ValidatePattern(entity.Password);

            if (!resultValidationPassword.IsValid)
                return resultValidationPassword;

            return this.Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(User entity)
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

        public void CommonValidation(User entity)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O {PropertyName} e obrigatorio.")
                .MaximumLength(100).WithMessage("O tamanho maximo do {PropertyName} e de {MaxLength} caracteres.");
            RuleFor(x => x.CompanyId)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.")
                .GreaterThan(0).WithMessage("O {PropertyName} e invalido.");
        }
    }
}
