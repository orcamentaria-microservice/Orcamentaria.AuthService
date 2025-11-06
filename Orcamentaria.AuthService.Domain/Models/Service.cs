using Orcamentaria.Lib.Domain.Entities;

namespace Orcamentaria.AuthService.Domain.Models
{
    public class Service
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool Active { get; set; } = true;
        public IEnumerable<Bootstrap> Bootstraps { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long UpdatedBy { get; set; }
    }
}
