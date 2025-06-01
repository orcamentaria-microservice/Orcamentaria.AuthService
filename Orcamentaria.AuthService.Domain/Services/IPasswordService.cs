using FluentValidation.Results;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IPasswordService
    {
        string Encript(string password);
        ValidationResult Validate(string password);
    }
}
