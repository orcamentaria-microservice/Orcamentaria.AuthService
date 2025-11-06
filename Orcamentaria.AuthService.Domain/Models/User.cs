using Orcamentaria.Lib.Domain.Entities;

namespace Orcamentaria.AuthService.Domain.Models
{
    public class User : TenantEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; } = true;
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
