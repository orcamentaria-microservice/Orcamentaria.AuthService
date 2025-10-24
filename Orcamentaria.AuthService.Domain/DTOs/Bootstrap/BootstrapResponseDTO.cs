namespace Orcamentaria.AuthService.Domain.DTOs.Bootstrap
{
    public class BootstrapResponseDTO
    {
        public long BootstrapId { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string BootstrapSecret { get; set; } = String.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
