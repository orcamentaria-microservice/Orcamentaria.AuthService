namespace Orcamentaria.AuthService.Domain.Services
{
    public interface ITokenService<T>
    {
        string Generate(T data);
        Task<long> Validate(string token);
    }
}
