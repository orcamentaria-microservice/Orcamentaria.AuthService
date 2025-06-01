using Orcamentaria.Lib.Domain.Enums;

namespace Orcamentaria.AuthService.Domain.DTOs.Permissions
{
    public class PermissionUpdateDTO
    {
        public ResourceEnum Resource { get; set; }
        public string Description { get; set; }
        public PermissionTypeEnum Type { get; set; }
    }
}
