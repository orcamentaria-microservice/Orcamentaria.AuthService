using Orcamentaria.Lib.Domain.Entities;

namespace Orcamentaria.AuthService.Domain.Models
{
    public class Bootstrap
    {
        public long Id { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime RevokedAt { get; set; }
        public bool Active { get; set; } = true;
        public string Hash { get; set; }
        public long ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
    }
}
