using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.Domain.Services
{
    public interface IServiceService
    {
        Response<ServiceResponseDTO> GetById(long id);
        Service GetByCredentials(string clientId, string clientSecret);
        Task<Response<ServiceResponseDTO>> Insert(ServiceInsertDTO dto);
        Task<Response<ServiceResponseDTO>> Update(long id, ServiceUpdateDTO dto);
        Task<Response<ServiceResponseDTO>> UpdateCredentials(long id);
    }
}
