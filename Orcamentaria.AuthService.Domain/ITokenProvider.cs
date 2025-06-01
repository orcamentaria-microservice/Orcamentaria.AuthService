namespace Orcamentaria.AuthService.Domain
{
    public interface ITokenProvider
    {
        public Task<string> GetTokenAsync();
    }
}
