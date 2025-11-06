using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IBootstrapService
    {
        Task<Bootstrap?> GetByIdAsync(long id);
        Task<Response<BootstrapResponseDTO>> GenerateBootstrapSecretAsync(long serviceId);
        Task<Response<long>> RevokeBootstrapSecretAsync(long serviceId);
    }
}
