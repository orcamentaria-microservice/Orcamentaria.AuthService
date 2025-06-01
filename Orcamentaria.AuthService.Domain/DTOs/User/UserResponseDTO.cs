namespace Orcamentaria.AuthService.Domain.DTOs.User
{
    public class UserResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long CompanyId { get; set; }
    }
}
