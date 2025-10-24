using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IBootstrapService
    {
        Response<BootstrapResponseDTO> GetById(long id);
        Task<Response<BootstrapResponseDTO>> CreateBootstrapSecret(long serviceId);
        Response<long> RevokeBootstrapSecret(long serviceId);
    }
}
