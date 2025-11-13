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

        public ValidationResult ValidateBeforeInsert(Service entity)
        {
            RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} nao deve ser informado.");

            RuleFor(x => x.ClientId)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.");

            RuleFor(x => x.ClientSecret)
                .NotNull().WithMessage("O {PropertyName} e obrigatorio.");

            return Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(Service entity)
        {
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

        public void CommonValidation(Service entity)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O {PropertyName} e obrigatorio.")
                .MaximumLength(100).WithMessage("O tamanho maximo do {PropertyName} e de {MaxLength} caracteres.");
        }
    }
}
