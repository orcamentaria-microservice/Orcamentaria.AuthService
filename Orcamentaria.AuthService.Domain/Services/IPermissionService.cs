using Orcamentaria.AuthService.Domain.DTOs.Permissions;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IPermissionService
    {
        Permission GetPermission(long id);
        Response<PermissionResponseDTO> GetById(long id);
        Response<IEnumerable<PermissionResponseDTO>> GetByResource(ResourceEnum resource);
        Response<IEnumerable<PermissionResponseDTO>> GetByType(PermissionTypeEnum type);
        Task<Response<PermissionResponseDTO>> Insert(PermissionInsertDTO dto);
        Task<Response<PermissionResponseDTO>> Update(long id, PermissionUpdateDTO dto);
    }
}
