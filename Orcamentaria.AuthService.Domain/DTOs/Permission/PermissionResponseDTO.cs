using Orcamentaria.Lib.Domain.Enums;

namespace Orcamentaria.AuthService.Domain.DTOs.Permission
{
    public class PermissionResponseDTO
    {
        public long Id { get; set; }
        public ResourceEnum Resource { get; set; }
        public string Description { get; set; }
        public PermissionTypeEnum Type { get; set; }
        public string IncrementalPermission { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long UpdatedBy { get; set; }
    }
}
