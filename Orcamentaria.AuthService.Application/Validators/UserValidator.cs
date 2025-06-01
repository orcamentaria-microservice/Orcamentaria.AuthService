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

        public UserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .MaximumLength(100).WithMessage("O tamanho máximo do {PropertyName} é de {MaxLength} caracteres.");
            RuleFor(x => x.CompanyId)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.")
                .GreaterThan(0).WithMessage("O {PropertyName} é inválido.");
            RuleFor(x => x.CreateAt)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.");
            RuleFor(x => x.UpdateAt.Date)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .LessThan(DateTime.Now.Date).WithMessage("A {PropertyName} não pode ser inferior a {ComparisonValue}.");
        }

        public ValidationResult ValidateBeforeInsert(User entity)
        {
            var validator = new UserValidator();

            validator.RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} não deve ser informado.");

            validator.RuleFor(x => x.Email)
                .EmailAddress().WithMessage("O {PropertyName} é inválido.")
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .Length(200).WithMessage("O {PropertyName} deve ter {MaxLength} caracteres.");

            var resultValidationPassword = _passwordService.Validate(entity.Password);

            if (!resultValidationPassword.IsValid)
                return resultValidationPassword;

            return this.Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(User entity)
        {
            var validator = new UserValidator();

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
