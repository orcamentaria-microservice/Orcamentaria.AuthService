namespace Orcamentaria.AuthService.Domain.DTOs.Service
{
    public class ServiceResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
