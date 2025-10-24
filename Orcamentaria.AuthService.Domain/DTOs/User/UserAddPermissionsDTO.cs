namespace Orcamentaria.AuthService.Domain.DTOs.User
{
    public class UserAddPermissionsDTO
    {
        public IEnumerable<long> PermissionIds { get; set; }
    }
}
