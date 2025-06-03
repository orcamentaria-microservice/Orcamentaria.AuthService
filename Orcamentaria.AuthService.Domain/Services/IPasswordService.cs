using FluentValidation.Results;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IPasswordService
    {
        string Encript(string password);
        bool PasswordIsValid(string password, string passwordEncript);
        ValidationResult ValidatePattern(string password);
    }
}
