using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ServiceController : Controller
    {
        private readonly IServiceService _service;

        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        [Authorize(Roles = "MASTER")]
        [HttpPost("Get", Name = "ServiceGet")]
        public async Task<Response<IEnumerable<ServiceResponseDTO>>?> GetAsync([FromBody] GridParams gridParams)
        {
            try
            {
                return await _service.GetAsync(gridParams);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER")]
        [HttpPost(Name = "ServiceInsert")]
        public async Task<Response<ServiceResponseDTO>> InsertAsync([FromBody] ServiceInsertDTO dto)
        {
            try
            {
                return await _service.InsertAsync(dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER")]
        [HttpPut("{id}", Name = "ServiceUpdate")]
        public async Task<Response<ServiceResponseDTO>> UpdateAsync(long id, [FromBody] ServiceUpdateDTO dto)
        {
            try
            {
                return await _service.UpdateAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER")]
        [HttpPut("UpdateCredentials/{id}", Name = "ServiceUpdateCredentials")]
        public async Task<Response<ServiceResponseDTO>> UpdateCredentialsAsync(long id)
        {
            try
            {
                return await _service.UpdateCredentialsAsync(id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
