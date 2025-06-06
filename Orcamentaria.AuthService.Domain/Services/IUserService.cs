using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IUserService
    {
        User? GetById(long id);
        User? GetUserByCredential(string email);
        Response<UserResponseDTO> GetByEmail(string email);
        Response<IEnumerable<UserResponseDTO>> GetByCompanyId();
        Task<Response<UserResponseDTO>> Insert(UserInsertDTO dto);
        Task<Response<UserResponseDTO>> Update(long id, UserUpdateDTO dto);
        Task<Response<UserResponseDTO>> UpdatePassword(long id, UserUpdatePasswordDTO dto);
        Task<Response<UserResponseDTO>> AddPermission(long userId, IEnumerable<long> permissionsId);
        Task<Response<UserResponseDTO>> RemovePermission(long userId, IEnumerable<long> permissionsId);
    }
}
