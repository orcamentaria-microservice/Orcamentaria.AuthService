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
                .NotEmpty().WithMessage("O {PropertyName} e obrigatorio.")
                .MaximumLength(100).WithMessage("O tamanho maximo do {PropertyName} e de {MaxLength} caracteres.");
        }

        public ValidationResult ValidateBeforeInsert(Service entity)
        {
            var validator = new ServiceValidator();

            validator.RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} nao deve ser informado.");

            validator.RuleFor(x => x.ClientId)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.");

            validator.RuleFor(x => x.ClientSecret)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.");

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
                   var entity = _repository.GetByIdAsync(x.Id).GetAwaiter().GetResult();

                   return entity is not null;

               }).WithMessage("Id nao encontrado.");

            return validator.Validate(entity);
        }
    }
}
