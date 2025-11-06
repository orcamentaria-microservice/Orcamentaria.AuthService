using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IServiceService
    {
        Task<Service?> GetByIdAsync(long id);
        Task<Response<IEnumerable<ServiceResponseDTO>>?> GetAsync(GridParams gridParams);
        Service GetByCredentials(string clientId, string clientSecret);
        Task<Response<ServiceResponseDTO>> InsertAsync(ServiceInsertDTO dto);
        Task<Response<ServiceResponseDTO>> UpdateAsync(long id, ServiceUpdateDTO dto);
        Task<Response<ServiceResponseDTO>> UpdateCredentialsAsync(long id);
    }
}
