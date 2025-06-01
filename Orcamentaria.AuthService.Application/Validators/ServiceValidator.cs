using FluentValidation;
using FluentValidation.Results;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Validators
{
    public class ServiceValidator : AbstractValidator<Service>, IValidatorEntity<Service>
    {
        private readonly IServiceRepository _repository;

        public ServiceValidator(
            IServiceRepository serviceRepository)
        {
            _repository = serviceRepository;
        }

        public ServiceValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .MaximumLength(100).WithMessage("O tamanho máximo do {PropertyName} é de {MaxLength} caracteres.");
        }

        public ValidationResult ValidateBeforeInsert(Service entity)
        {
            var validator = new ServiceValidator();

            validator.RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} não deve ser informado.");

            validator.RuleFor(x => x.ClientId)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.");

            validator.RuleFor(x => x.ClientSecret)
                .NotNull().WithMessage("O {PropertyName} é obrigatório.");

            return validator.Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(Service entity)
        {
            var validator = new ServiceValidator();

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
