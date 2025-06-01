namespace Orcamentaria.AuthService.Domain.Models
{
    public class Service
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool Active { get; set; } = true;
    }
}
