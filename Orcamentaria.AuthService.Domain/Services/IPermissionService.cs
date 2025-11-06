using Orcamentaria.AuthService.Domain.DTOs.Permission;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IPermissionService
    {
        Task<Permission?> GetByIdAsync(long id);
        Task<Response<IEnumerable<PermissionResponseDTO>>?> GetAsync(GridParams gridParams);
        Task<Response<PermissionResponseDTO>> InsertAsync(PermissionInsertDTO dto);
        Task<Response<PermissionResponseDTO>> UpdateAsync(long id, PermissionUpdateDTO dto);
    }
}
