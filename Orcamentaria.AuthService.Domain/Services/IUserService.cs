using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(long id);
        User? GetByEmail(string email);
        Task<Response<IEnumerable<UserResponseDTO>>?> GetAsync(GridParams gridParams);
        Task<Response<UserResponseDTO>> InsertAsync(UserInsertDTO dto);
        Task<Response<UserResponseDTO>> UpdateAsync(long id, UserUpdateDTO dto);
        Task<Response<UserResponseDTO>> UpdatePasswordAsync(long id, UserUpdatePasswordDTO dto);
        Task<Response<UserResponseDTO>> AddPermissionsAsync(long userId, UserAddPermissionsDTO dto);
        Task<Response<UserResponseDTO>> RemovePermissionsAsync(long userId, UserRemovePermissionsDTO dto);
    }
}
