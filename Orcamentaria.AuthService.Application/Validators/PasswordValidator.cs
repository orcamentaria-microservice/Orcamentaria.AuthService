using FluentValidation;
using FluentValidation.Results;
using System;
using System.Text.RegularExpressions;

namespace Orcamentaria.AuthService.Application.Validators
{
    public class PasswordValidator : AbstractValidator<string>
    {
        private static string pattern = @"^(?!.*\s)(?=(?:.*\d){3,})(?=.*[A-Z])(?=.*[\W_]).{8,}$";
        public ValidationResult Validate(string passwordValidate)
        {
            RuleFor(password => password)
                .NotEmpty().WithMessage("O {PropertyName} deve ser informado.");

            RuleFor(password => password)
               .Must((x, cancelation) =>
               {
                   return Regex.IsMatch(x, pattern);
               }).WithMessage(
                "O Password deve ter pelo menos 8 caracteres, " +
                "sendo eles pelo menos 1 letra maiuscula," +
                "1 caracter especial e 3 números.");

            return this.Validate(passwordValidate);
        }
    }
}
