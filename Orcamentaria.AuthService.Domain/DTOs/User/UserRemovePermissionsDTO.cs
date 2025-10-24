namespace Orcamentaria.AuthService.Domain.DTOs.User
{
    public class UserRemovePermissionsDTO
    {
        public IEnumerable<long> PermissionIds { get; set; }
    }
}
