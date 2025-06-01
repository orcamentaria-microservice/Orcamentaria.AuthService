namespace Orcamentaria.AuthService.Domain.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public long CompanyId { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
