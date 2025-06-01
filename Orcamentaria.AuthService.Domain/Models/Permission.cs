using Orcamentaria.Lib.Domain.Enums;

namespace Orcamentaria.AuthService.Domain.Models
{
    public class Permission
    {
        public long Id { get; set; }
        public ResourceEnum Resource { get; set; }
        public string Description { get; set; }
        public PermissionTypeEnum Type { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public IEnumerable<User> Users { get; set; }
    }
}
